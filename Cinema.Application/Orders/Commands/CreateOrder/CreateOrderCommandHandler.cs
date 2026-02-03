using Cinema.Application.Common.Interfaces;
using Cinema.Application.Common.Models.Payments;
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
    ISeatLockingService lockingService,
    IPriceCalculator priceCalculator,
    IPaymentService paymentService,
    ILogger<CreateOrderCommandHandler> logger
    ) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId == null) return Result.Failure<Guid>(new Error("Auth.Required", "User not authenticated"));

        var sessionId = new EntityId<Session>(request.SessionId);
        var session = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        if (session == null) 
            return Result.Failure<Guid>(new Error("Session.NotFound", "Session not found"));
            
        if (session.StartTime <= DateTime.UtcNow)
             return Result.Failure<Guid>(new Error("Session.Started", "Cannot book tickets for a started session."));
        
        var seatIdsToCheck = request.SeatIds.Select(id => new EntityId<Seat>(id)).ToList();
        var seats = await context.Seats
            .Include(s => s.SeatType)
            .Where(s => seatIdsToCheck.Contains(s.Id) && s.HallId == session.HallId)
            .ToListAsync(ct);
        
        var unavailableSeat = seats.FirstOrDefault(s => s.Status != SeatStatus.Active);
        if (unavailableSeat != null)
        {
            return Result.Failure<Guid>(new Error("Order.SeatUnavailable", 
                $"Seat {unavailableSeat.RowLabel}-{unavailableSeat.Number} is currently unavailable (Status: {unavailableSeat.Status})."));
        }

        if (seats.Count != request.SeatIds.Count)
            return Result.Failure<Guid>(new Error("Order.InvalidSeats", "Some seats do not exist in this hall."));

        foreach (var seat in seats)
        {
            var hasLock = await lockingService.ValidateLockAsync(request.SessionId, seat.Id.Value, userId.Value, ct);
            if (!hasLock)
            {
                return Result.Failure<Guid>(new Error("Order.LockExpired", $"Lock for seat {seat.RowLabel}-{seat.Number} has expired."));
            }
        }

        var soldSeats = await context.Tickets
            .AsNoTracking()
            .Where(t => t.SessionId == sessionId && 
                        seatIdsToCheck.Contains(t.SeatId) && 
                        (t.TicketStatus == TicketStatus.Valid || t.TicketStatus == TicketStatus.Used))
            .AnyAsync(ct);

        if (soldSeats)
             return Result.Failure<Guid>(new Error("Order.SeatsSold", "One or more seats are already sold."));

        decimal totalAmount = 0;
        var ticketPrices = new Dictionary<EntityId<Seat>, decimal>();

        try
        {
            foreach (var seat in seats)
            {
                var price = await priceCalculator.CalculatePriceAsync(
                    session.PricingId,
                    seat.SeatTypeId, 
                    session.StartTime, 
                    ct
                );
                totalAmount += price;
                ticketPrices[seat.Id] = price;
            }
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(new Error("Pricing.Error", ex.Message));
        }
        
        var paymentResult = await paymentService.ProcessPaymentAsync(totalAmount, "UAH", request.PaymentToken, ct);

        if (!paymentResult.IsSuccess)
        {
            logger.LogWarning("Payment failed for User {UserId}", userId);
            return Result.Failure<Guid>(new Error("Payment.Failed", paymentResult.ErrorMessage ?? "Payment declined."));
        }
        
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
            
            try
            {
                var orderId = EntityId<Order>.New();
                var order = Order.New(orderId, totalAmount, userId.Value, sessionId);
                
                order.MarkAsPaid(paymentResult.TransactionId!);

                context.Orders.Add(order);

                foreach (var seat in seats)
                {
                    var ticket = Ticket.New(
                        EntityId<Ticket>.New(),
                        ticketPrices[seat.Id],
                        TicketStatus.Valid,
                        orderId,
                        sessionId,
                        seat.Id
                    );
                    context.Tickets.Add(ticket);
                }

                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                
                _ = Task.Run(async () => 
                {
                    foreach (var seatId in request.SeatIds)
                    {
                        await lockingService.UnlockSeatAsync(request.SessionId, seatId, userId.Value);
                    }
                }, CancellationToken.None);

                return Result.Success(order.Id.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                logger.LogError(ex, "DB failed after payment. Refund needed.");

                await paymentService.RefundPaymentAsync(paymentResult.TransactionId!, ct);
                return Result.Failure<Guid>(new Error("Order.SystemError", "System error. Payment refunded."));
            }
        });
    }
}