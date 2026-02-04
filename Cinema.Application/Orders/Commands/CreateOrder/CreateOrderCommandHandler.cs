using Cinema.Application.Common.Interfaces;
using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Cinema.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using Cinema.Domain.Exceptions;

namespace Cinema.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    IPriceCalculator priceCalculator,
    IPaymentService paymentService,
    ISeatLockingService seatLockingService,
    ILogger<CreateOrderCommandHandler> logger
    ) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId == null) 
            return Result.Failure<Guid>(new Error("Auth.Required", "User not authenticated"));
        
        foreach (var seatId in request.SeatIds)
        {
            var hasLock = await seatLockingService.ValidateLockAsync(request.SessionId, seatId, userId.Value, ct);
            if (!hasLock)
                return Result.Failure<Guid>(new Error("Order.LockExpired", "Seat lock expired or invalid."));
        }

        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
            try
            {
                var rawSeatIds = request.SeatIds.ToArray();

                var seatIdsParam = string.Join(",", rawSeatIds.Select(id => $"'{id}'"));

                var seats = await context.Seats
                    .FromSqlRaw($"SELECT * FROM seats WHERE id IN ({seatIdsParam}) FOR UPDATE NOWAIT")
                    .ToListAsync(ct);

                if (seats.Count != request.SeatIds.Count)
                    return Result.Failure<Guid>(new Error("Order.SeatsNotFound", "Some seats unavailable or ID mismatch."));

                if (seats.Any(s => s.Status != SeatStatus.Active))
                     return Result.Failure<Guid>(new Error("Order.SeatsNotActive", "One or more seats are not active."));
                
                var soldSeats = await context.Tickets
                    .AnyAsync(t => 
                        t.SessionId == new EntityId<Session>(request.SessionId) && 
                        request.SeatIds.Contains(t.SeatId.Value) && 
                        (t.TicketStatus == TicketStatus.Valid || t.TicketStatus == TicketStatus.Used), ct);

                if (soldSeats)
                     return Result.Failure<Guid>(new Error("Order.SeatsSold", "One or more seats are already sold."));

                var sessionId = new EntityId<Session>(request.SessionId);
                var session = await context.Sessions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

                if (session == null) 
                    return Result.Failure<Guid>(new Error("Session.NotFound", "Session not found"));

                var pricing = await context.Pricings
                    .Include(p => p.PricingItems)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == session.PricingId, ct);
                
                if (pricing == null)
                    return Result.Failure<Guid>(new Error("Pricing.NotFound", "Pricing policy missing."));

                var prices = new Dictionary<EntityId<Seat>, decimal>();
                foreach (var seat in seats)
                {
                    try 
                    {
                        var price = priceCalculator.CalculatePrice(pricing, seat.SeatTypeId, session.StartTime);
                        prices[seat.Id] = price;
                    }
                    catch (DomainException dex)
                    {
                        return Result.Failure<Guid>(new Error("Pricing.Error", dex.Message));
                    }
                }
                
                var order = Order.Create(userId.Value, session, seats, prices);
                
                context.Orders.Add(order);
                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);
                
                var paymentResult = await paymentService.ProcessPaymentAsync(order.TotalAmount, "UAH", request.PaymentToken, ct);

                if (!paymentResult.IsSuccess)
                {

                    order.MarkAsFailed();
                    context.Orders.Update(order);
                    await context.SaveChangesAsync(ct);
                    
                    return Result.Failure<Guid>(new Error("Payment.Failed", paymentResult.ErrorMessage ?? "Payment declined."));
                }

                order.MarkAsPaid(paymentResult.TransactionId!);
                context.Orders.Update(order);
                await context.SaveChangesAsync(ct);
                
                _ = Task.Run(async () => 
                {
                    foreach (var seatId in request.SeatIds)
                        await seatLockingService.UnlockSeatAsync(request.SessionId, seatId, userId.Value);
                }, CancellationToken.None);

                return Result.Success(order.Id.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                logger.LogError(ex, "Order creation transaction failed.");
                
                if (ex.InnerException?.Message.Contains("55P03") == true)
                {
                    return Result.Failure<Guid>(new Error("Order.Concurrency", "Seats are currently being booked by someone else. Please try again."));
                }

                return Result.Failure<Guid>(new Error("Order.SystemError", "An unexpected error occurred."));
            }
        });
    }
}