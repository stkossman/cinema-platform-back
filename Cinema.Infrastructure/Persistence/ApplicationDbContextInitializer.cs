using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cinema.Infrastructure.Persistence;

public class ApplicationDbContextInitializer(
    ILogger<ApplicationDbContextInitializer> logger,
    ApplicationDbContext dbContext)
{
    public async Task InitialiseAsync()
    {
        try
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while initialising the database.");
            throw;
        }
    }
    
    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // (Seat Types)
        var standardTypeId = new EntityId<SeatType>(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var vipTypeId = new EntityId<SeatType>(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        if (!await dbContext.SeatTypes.AnyAsync())
        {
            var standard = SeatType.New(
                standardTypeId, 
                "Standard", 
                "Звичайне зручне крісло"
            );
            
            var vip = SeatType.New(
                vipTypeId, 
                "VIP", 
                "Шкіряне крісло-реклайнер"
            );

            await dbContext.SeatTypes.AddRangeAsync(standard, vip);
            await dbContext.SaveChangesAsync();
        }

        // (Genres)
        if (!await dbContext.Genres.AnyAsync())
        {
            var action = Genre.New(new EntityId<Genre>(Guid.NewGuid()), 28, "Action", "action");
            var sciFi = Genre.New(new EntityId<Genre>(Guid.NewGuid()), 878, "Sci-Fi", "sci-fi");
            var drama = Genre.New(new EntityId<Genre>(Guid.NewGuid()), 18, "Drama", "drama");

            await dbContext.Genres.AddRangeAsync(action, sciFi, drama);
            await dbContext.SaveChangesAsync();
        }

        // (Movies)
        var inceptionId = new EntityId<Movie>(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        
        if (!await dbContext.Movies.AnyAsync())
        {
            var inception = Movie.New(
                inceptionId,
                27205,
                "Inception",
                148,
                8.8m,
                "https://image.tmdb.org/t/p/w500/9gk7admal4zl67YrxIo16EO00ww.jpg",
                "https://www.youtube.com/watch?v=YoHD9XEInc0"
            );

            var matrix = Movie.New(
                new EntityId<Movie>(Guid.NewGuid()),
                603,
                "The Matrix",
                136,
                8.7m,
                "https://image.tmdb.org/t/p/w500/f89U3ADr1oiB1s9GkdPOEpFfCHG.jpg",
                "https://www.youtube.com/watch?v=vKQi3bBA1y8"
            );

            await dbContext.Movies.AddRangeAsync(inception, matrix);
            await dbContext.SaveChangesAsync();
        }

        // (Pricing)
        var defaultPricingId = new EntityId<Pricing>(Guid.Parse("44444444-4444-4444-4444-444444444444"));
        if (!await dbContext.Pricings.AnyAsync())
        {
            var pricing = Pricing.New(
                defaultPricingId, 
                "Base Tariff 2026"
            );
            
            await dbContext.Pricings.AddAsync(pricing);
            await dbContext.SaveChangesAsync();
        }

        // (Halls & Seats)
        if (!await dbContext.Halls.AnyAsync())
        {
            var hallId = new EntityId<Hall>(Guid.Parse("55555555-5555-5555-5555-555555555555"));
            var rows = 10;
            var seatsPerRow = 15;
            var capacity = rows * seatsPerRow;

            var hall = Hall.New(
                hallId, 
                "IMAX Hall 1", 
                capacity, 
                new List<HallTechnology>()
            );

            dbContext.Halls.Add(hall);

            var seats = new List<Seat>();
            for (int r = 1; r <= rows; r++)
            {
                for (int n = 1; n <= seatsPerRow; n++)
                {
                    var typeId = (r == rows) ? vipTypeId : standardTypeId;

                    var seat = Seat.New(
                        id: new EntityId<Seat>(Guid.NewGuid()),
                        rowLabel: r.ToString(),
                        number: n,
                        gridX: n,
                        gridY: r,
                        status: SeatStatus.Active,
                        hallId: hallId,
                        seatTypeId: typeId
                    );
                    seats.Add(seat);
                }
            }

            dbContext.Seats.AddRange(seats);
            await dbContext.SaveChangesAsync();
        }
    }
}