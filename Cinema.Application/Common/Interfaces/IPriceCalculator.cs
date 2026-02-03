using Cinema.Domain.Common;
using Cinema.Domain.Entities;

namespace Cinema.Application.Common.Interfaces;

public interface IPriceCalculator
{
    Task<decimal> CalculatePriceAsync(
        EntityId<Pricing> pricingId, 
        EntityId<SeatType> seatTypeId, 
        DateTime sessionStartTime, 
        CancellationToken ct = default);
}