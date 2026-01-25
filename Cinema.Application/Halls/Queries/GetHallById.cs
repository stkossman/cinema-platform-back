using Cinema.Application.Common.Interfaces;
using Cinema.Application.Halls.Dtos;
using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Halls.Queries;

public record GetHallByIdQuery(Guid Id) : IRequest<Result<HallDto>>;

public class GetHallByIdQueryHandler : IRequestHandler<GetHallByIdQuery, Result<HallDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHallByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<HallDto>> Handle(GetHallByIdQuery request, CancellationToken cancellationToken)
    {
        var hallId = new EntityId<Hall>(request.Id);

        var hall = await _context.Halls
            .AsNoTracking()
            .Include(h => h.Seats)
            .ThenInclude(s => s.SeatType)
            .FirstOrDefaultAsync(h => h.Id == hallId, cancellationToken);

        if (hall == null)
        {
            return Result.Failure<HallDto>(new Error("Hall.NotFound", "Hall not found"));
        }

        return Result.Success(HallDto.FromDomainModel(hall));
    }
}