using Cinema.Application.Common.Interfaces;
using Cinema.Application.Orders.Dtos;
using Cinema.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Orders.Queries.GetUserOrders;

public record GetUserOrdersQuery(Guid UserId) : IRequest<Result<List<OrderDto>>>;

public class GetUserOrdersQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetUserOrdersQuery, Result<List<OrderDto>>>
{
    public async Task<Result<List<OrderDto>>> Handle(GetUserOrdersQuery request, CancellationToken ct)
    {
        
        var orders = await context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.BookingDate)
            .Select(o => new OrderDto(
                o.Id.Value,
                o.BookingDate,
                o.TotalAmount,
                o.Status.ToString(),
                context.Tickets
                    .Where(t => t.OrderId == o.Id)
                    .Select(t => new TicketDto(
                        t.Id.Value,
                        t.Session!.Movie!.Title,             
                        t.Session.Movie.PosterUrl ?? "",        
                        t.Session.Hall!.Name,               
                        t.Seat!.RowLabel,                   
                        t.Seat.Number,                     
                        t.Seat.SeatType!.Name,              
                        t.PriceSnapshot, 
                        t.Session.StartTime,
                        t.TicketStatus.ToString(),
                        t.Id.Value.ToString().Substring(0, 8).ToUpper() 
                    ))
                    .ToList()
            ))
            .ToListAsync(ct);

        return Result.Success(orders);
    }
}