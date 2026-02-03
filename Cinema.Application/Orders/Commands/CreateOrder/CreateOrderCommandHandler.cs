using Cinema.Application.Common.Interfaces;
using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Cinema.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        
        var session = await context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == new EntityId<Session>(request.SessionId), ct);

        if (session == null) 
            return Result.Failure<Guid>(new Error("Session.NotFound", "Session not found"));
        
        var seatIds = request.SeatIds.Select(id => new EntityId<Seat>(id)).ToList();
        var seats = await context.Seats
            .Where(s => seatIds.Contains(s.Id) && s.HallId == session.HallId)
            .ToListAsync(ct);

        if (seats.Count != request.SeatIds.Count)
            return Result.Failure<Guid>(new Error("Order.InvalidSeats", "Some selected seats do not exist or belong to another hall."));
        
        foreach (var seat in seats)
        {
            var hasLock = await seatLockingService.ValidateLockAsync(request.SessionId, seat.Id.Value, userId.Value, ct);
            if (!hasLock)
            {
                return Result.Failure<Guid>(new Error("Order.LockExpired", $"Lock for seat {seat.RowLabel}-{seat.Number} has expired."));
            }
        }

        var soldSeats = await context.Tickets
            .AsNoTracking()
            .AnyAsync(t => t.SessionId == session.Id && 
                           seatIds.Contains(t.SeatId) && 
                           (t.TicketStatus == TicketStatus.Valid || t.TicketStatus == TicketStatus.Used), ct);

        if (soldSeats)
             return Result.Failure<Guid>(new Error("Order.SeatsSold", "One or more seats are already sold."));

        var prices = new Dictionary<EntityId<Seat>, decimal>();
        try
        {
            foreach (var seat in seats)
            {
                prices[seat.Id] = await priceCalculator.CalculatePriceAsync(session.PricingId, seat.SeatTypeId, session.StartTime, ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pricing calculation failed");
            return Result.Failure<Guid>(new Error("Pricing.Error", "Failed to calculate ticket prices."));
        }
        
        Order order;
        try
        {
            order = Order.Create(userId.Value, session, seats, prices);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(new Error("Order.DomainError", ex.Message));
        }
        
        var paymentResult = await paymentService.ProcessPaymentAsync(order.TotalAmount, "UAH", request.PaymentToken, ct);

        if (!paymentResult.IsSuccess)
        {
            logger.LogWarning("Payment failed for User {UserId}: {Error}", userId, paymentResult.ErrorMessage);
            return Result.Failure<Guid>(new Error("Payment.Failed", paymentResult.ErrorMessage ?? "Payment declined."));
        }
        
        order.MarkAsPaid(paymentResult.TransactionId!);

        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
            
            try
            {
                context.Orders.Add(order);

                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                
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
                logger.LogError(ex, "Database commit failed after payment. Refunding...");
                
                await paymentService.RefundPaymentAsync(paymentResult.TransactionId!, ct);
                
                return Result.Failure<Guid>(new Error("Order.SystemError", "System error occurred. Payment has been refunded."));
            }
        });
    }
}