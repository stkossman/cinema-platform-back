using Cinema.Domain.Shared;
using MediatR;

namespace Cinema.Application.Sessions.Commands.CreateSession;

public record CreateSessionCommand(
    Guid MovieId,
    Guid HallId,
    Guid PricingId,
    DateTime StartTime
) : IRequest<Result<Guid>>;