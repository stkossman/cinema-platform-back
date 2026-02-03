using Cinema.Application.Common.Interfaces;
using Cinema.Application.Orders.Dtos;
using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Tickets.Queries.GetTicketDetails;

public record GetTicketDetailsQuery(Guid TicketId) : IRequest<Result<TicketDto>>;

public class GetTicketDetailsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<GetTicketDetailsQuery, Result<TicketDto>>
{
    public async Task<Result<TicketDto>> Handle(GetTicketDetailsQuery request, CancellationToken ct)
    {
        var ticketId = new EntityId<Ticket>(request.TicketId);
        var userId = currentUser.UserId;
        
        var ticketData = await context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Select(t => new 
            {
                Ticket = t,
                UserId = t.Order!.UserId,
                MovieTitle = t.Session!.Movie!.Title,
                PosterUrl = t.Session.Movie.PosterUrl,
                HallName = t.Session.Hall!.Name,
                Row = t.Seat!.RowLabel,
                Number = t.Seat.Number,
                SeatType = t.Seat.SeatType!.Name,
                SessionStart = t.Session.StartTime
            })
            .FirstOrDefaultAsync(ct);

        if (ticketData == null)
            return Result.Failure<TicketDto>(new Error("Ticket.NotFound", "Ticket not found."));

        if (ticketData.UserId != userId)
             return Result.Failure<TicketDto>(new Error("Ticket.AccessDenied", "This ticket belongs to another user."));

        var dto = new TicketDto(
            ticketData.Ticket.Id.Value,
            ticketData.MovieTitle,
            ticketData.PosterUrl ?? "",
            ticketData.HallName,
            ticketData.Row,
            ticketData.Number,
            ticketData.SeatType,
            ticketData.Ticket.PriceSnapshot,
            ticketData.SessionStart,
            ticketData.Ticket.TicketStatus.ToString(),
            ticketData.Ticket.Id.Value.ToString()
        );

        return Result.Success(dto);
    }
}