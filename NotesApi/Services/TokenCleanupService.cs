public class TokenCleanupService : IHostedService, IDisposable
{
    private IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<TokenCleanupService> _logger;
    private Timer? _timer = null;
    private int _daysAfterCleanupIsRequired = 10;

    public TokenCleanupService(ILogger<TokenCleanupService> logger, IConfiguration config, IRefreshTokenRepository refreshTokenRepository)
    {
        _logger = logger;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token Cleanup Service running.");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogInformation($"Deleting expired tokens present in db for >= {_daysAfterCleanupIsRequired} days");

        var allData = await _refreshTokenRepository.GetAllAsync();
        var filtered = allData.Where(x => (DateTime.UtcNow - x.ExpiresAt).TotalDays >= _daysAfterCleanupIsRequired);
        await _refreshTokenRepository.DeleteAsync(filtered);

        _logger.LogInformation($"Deleted expired tokens from db which were there for >= {_daysAfterCleanupIsRequired} days");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token Cleanup Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}