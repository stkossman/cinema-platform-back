using Cinema.Application.Common.Interfaces;
using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using Cinema.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cinema.Infrastructure.Services;

public class PriceCalculator(
    IApplicationDbContext context,
    ILogger<PriceCalculator> logger) 
    : IPriceCalculator
{
    public async Task<decimal> CalculatePriceAsync(
        EntityId<Pricing> pricingId, 
        EntityId<SeatType> seatTypeId, 
        DateTime sessionStartTime, 
        CancellationToken ct = default)
    {
        var pricingData = await context.Pricings
            .AsNoTracking()
            .Include(p => p.PricingItems)
            .ThenInclude(pi => pi.SeatType)
            .FirstOrDefaultAsync(p => p.Id == pricingId, ct);

        if (pricingData == null)
        {
            logger.LogError("Pricing policy {PricingId} not found.", pricingId);
            throw new DomainException($"Pricing policy not found.");
        }

        var relevantItems = pricingData.PricingItems?
            .Where(pi => pi.SeatTypeId == seatTypeId)
            .ToList();

        if (relevantItems == null || !relevantItems.Any())
        {
            var seatTypeName = await context.SeatTypes
                .Where(st => st.Id == seatTypeId)
                .Select(st => st.Name)
                .FirstOrDefaultAsync(ct) ?? "Unknown";

            throw new PriceNotConfiguredException(pricingData.Name, seatTypeName, sessionStartTime);
        }
        
        var requestTime = sessionStartTime.TimeOfDay;
        var requestDay = sessionStartTime.DayOfWeek;

        var bestMatch = relevantItems
            .Where(item => item.DayOfWeek == requestDay)
            .Where(item => IsTimeApplicable(item, requestTime))
            .OrderByDescending(item => item.StartTime.HasValue && item.EndTime.HasValue) 
            .FirstOrDefault();

        if (bestMatch != null)
        {
            return bestMatch.Price;
        }
        
        var seatType = relevantItems.First().SeatType?.Name ?? "Unknown";
        throw new PriceNotConfiguredException(pricingData.Name, seatType, sessionStartTime);
    }
    
    private static bool IsTimeApplicable(PricingItem item, TimeSpan sessionTime)
    {
        if (!item.StartTime.HasValue || !item.EndTime.HasValue)
            return true;

        var start = item.StartTime.Value.TimeOfDay;
        var end = item.EndTime.Value.TimeOfDay;
        
        if (start <= end)
        {
            return sessionTime >= start && sessionTime <= end;
        }
        else
        {
            return sessionTime >= start || sessionTime <= end;
        }
    }
}