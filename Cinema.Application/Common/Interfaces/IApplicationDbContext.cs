using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cinema.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Hall> Halls { get; }
    DbSet<Seat> Seats { get; }
    DbSet<SeatType> SeatTypes { get; }
    DbSet<Session> Sessions { get; }
    DbSet<Movie> Movies { get; }
    DbSet<Genre> Genres { get; }
    DbSet<Pricing> Pricing { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}