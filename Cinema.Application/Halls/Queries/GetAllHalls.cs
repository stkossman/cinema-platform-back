using Cinema.Application.Common.Interfaces;
using Cinema.Application.Halls.Dtos;
using Cinema.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Halls.Queries;

public record GetAllHallsQuery : IRequest<Result<List<HallDto>>>;

public class GetAllHallsQueryHandler : IRequestHandler<GetAllHallsQuery, Result<List<HallDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllHallsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<HallDto>>> Handle(GetAllHallsQuery request, CancellationToken cancellationToken)
    {
        var halls = await _context.Halls
            .AsNoTracking()
            .Select(h => HallDto.FromDomainModel(h))
            .ToListAsync(cancellationToken);

        return Result.Success(halls);
    }
}