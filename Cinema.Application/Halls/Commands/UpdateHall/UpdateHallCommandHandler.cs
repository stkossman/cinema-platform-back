using Cinema.Application.Common.Interfaces;
using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Exceptions;
using Cinema.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Halls.Commands.UpdateHall;

public class UpdateHallCommandHandler : IRequestHandler<UpdateHallCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateHallCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateHallCommand request, CancellationToken cancellationToken)
    {
        var hallId = new EntityId<Hall>(request.HallId);

        var hall = await _context.Halls
            .FirstOrDefaultAsync(h => h.Id == hallId, cancellationToken);

        if (hall == null)
        {
            return Result.Failure(new Error("Hall.NotFound", "Hall not found."));
        }

        try
        {
            hall.Update(request.Name);
            
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error("Hall.UpdateFailed", ex.Message));
        }

        return Result.Success();
    }
}