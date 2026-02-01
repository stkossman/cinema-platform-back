using Cinema.Application.Common.Interfaces;
using Cinema.Application.Technologies.Dtos;
using Cinema.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Technologies.Queries.GetAllTechnologies;

public class GetAllTechnologiesQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetAllTechnologiesQuery, Result<List<TechnologyDto>>>
{
    public async Task<Result<List<TechnologyDto>>> Handle(GetAllTechnologiesQuery request, CancellationToken ct)
    {
        var technologies = await context.Technologies
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => TechnologyDto.FromDomain(t))
            .ToListAsync(ct);

        return Result.Success(technologies);
    }
}