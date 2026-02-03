using Cinema.Domain.Shared;
using MediatR;

namespace Cinema.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid SessionId, 
    List<Guid> SeatIds,
    string PaymentToken 
) : IRequest<Result<Guid>>;