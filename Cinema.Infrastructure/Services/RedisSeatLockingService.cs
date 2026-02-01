using Cinema.Application.Common.Interfaces;
using Cinema.Domain.Shared;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace Cinema.Infrastructure.Services;

public class RedisSeatLockingService : ISeatLockingService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ResiliencePipeline _resiliencePipeline;
    private const int LockTimeMinutes = 10; 
    // TODO: Винести в appsettings.json у майбутньому

    public RedisSeatLockingService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<RedisTimeoutException>()
                    .Handle<RedisConnectionException>(),
                
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            })
            .AddTimeout(TimeSpan.FromSeconds(2))
            .Build();
    }

    public async Task<Result> LockSeatAsync(Guid sessionId, Guid seatId, Guid userId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(sessionId, seatId);
        var value = userId.ToString();
        var expiry = TimeSpan.FromMinutes(LockTimeMinutes);
        
        try 
        {
            bool wasSet = await _resiliencePipeline.ExecuteAsync(async token => 
                await db.StringSetAsync(key, value, expiry, When.NotExists), ct);

            if (!wasSet)
            {
                var currentOwner = await db.StringGetAsync(key);
                if (currentOwner == value)
                {
                    await db.KeyExpireAsync(key, expiry);
                    return Result.Success();
                }

                return Result.Failure(new Error("Seat.Locked", "Seat is already locked by another user."));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Redis.Error", "Temporary system failure. Please try again later."));
        }
    }

    public async Task<Result> UnlockSeatAsync(Guid sessionId, Guid seatId, Guid userId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(sessionId, seatId);
        
        var script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        try
        {
            await _resiliencePipeline.ExecuteAsync(async token => 
                await db.ScriptEvaluateAsync(script, new RedisKey[] { key }, new RedisValue[] { userId.ToString() }), ct);
            
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("Redis.Error", "Failed to unlock seat."));
        }
    }

    private static string GetKey(Guid sessionId, Guid seatId) 
        => $"lock:s:{sessionId}:st:{seatId}";
}