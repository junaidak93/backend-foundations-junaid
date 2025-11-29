using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

public class RateLimiterConfig(ILogger<RateLimiterConfig> logger)
{
    private ILogger<RateLimiterConfig> _logger = logger;

    public void GetConfig(RateLimiterOptions limiterOptions)
    {
        limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
        {
            var ip = http.Connection.RemoteIpAddress?.ToString() ?? http.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            var userKey = http.User.Identity?.Name ?? ip?.ToString() ?? "anonymous";

            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: userKey,
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 50,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 10,
                    QueueLimit = 2,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });   
        });

        limiterOptions.OnRejected = (context, cancellationToken) =>
        {
            var http = context.HttpContext;
            var ip = http.Connection.RemoteIpAddress?.ToString() ?? http.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            var userKey = http.User.Identity?.Name ?? ip?.ToString() ?? "anonymous";

            // Handle rejected requests (e.g., set status code 429 Too Many Requests)
            _logger.LogInformation($"{userKey} was blocked by rate limiting.");
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return ValueTask.CompletedTask;
        };
    }
}