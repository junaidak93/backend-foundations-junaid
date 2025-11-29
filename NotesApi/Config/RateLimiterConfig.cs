using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

public class RateLimiterConfig
{
    public void GetConfig(RateLimiterOptions limiterOptions)
    {
        limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: http.User.Identity?.Name ?? http.Connection.RemoteIpAddress?.ToString()!,
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 50,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 10,
                    QueueLimit = 2,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                })
            );

        limiterOptions.OnRejected = (context, cancellationToken) =>
        {
            // Handle rejected requests (e.g., set status code 429 Too Many Requests)
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return ValueTask.CompletedTask;
        };
    }
}