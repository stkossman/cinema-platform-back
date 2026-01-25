using Cinema.Application.Common.Interfaces;
using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Halls.Commands.DeleteHall;

public class DeleteHallCommandHandler : IRequestHandler<DeleteHallCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteHallCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteHallCommand request, CancellationToken cancellationToken)
    {
        var hallId = new EntityId<Hall>(request.HallId);

        var hall = await _context.Halls
            .FirstOrDefaultAsync(h => h.Id == hallId, cancellationToken);

        if (hall == null)
        {
            return Result.Failure(new Error("Hall.NotFound", "Hall not found."));
        }

        hall.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}