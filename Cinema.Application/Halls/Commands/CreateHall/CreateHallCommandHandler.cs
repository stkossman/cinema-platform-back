using Cinema.Application.Common.Interfaces;
using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Cinema.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Halls.Commands.CreateHall;

public class CreateHallCommandHandler : IRequestHandler<CreateHallCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateHallCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateHallCommand request, CancellationToken cancellationToken)
    {
        var seatTypeId = new EntityId<SeatType>(request.SeatTypeId);
        var seatTypeExists = await _context.SeatTypes.AnyAsync(x => x.Id == seatTypeId, cancellationToken);
        
        if (!seatTypeExists)
        {
            return Result<Guid>.Failure(new Error("Hall.InvalidSeatType", "Seat type not found."));
        }

        var hallId = new EntityId<Hall>(Guid.NewGuid());
        var totalCapacity = request.Rows * request.SeatsPerRow;
        
        var hall = Hall.New(hallId, request.Name, totalCapacity, new List<HallTechnology>());
        _context.Halls.Add(hall);
        
        var seats = new List<Seat>();
        for (int row = 1; row <= request.Rows; row++)
        {
            for (int number = 1; number <= request.SeatsPerRow; number++)
            {
                var seat = Seat.New(
                    id: new EntityId<Seat>(Guid.NewGuid()),
                    rowLabel: row.ToString(),
                    number: number,
                    gridX: number,
                    gridY: row,
                    status: SeatStatus.Active,
                    hallId: hallId,
                    seatTypeId: seatTypeId
                );
                seats.Add(seat);
            }
        }
        
        _context.Seats.AddRange(seats);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(hallId.Value);
    }
}