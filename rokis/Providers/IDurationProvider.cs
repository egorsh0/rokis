using rokis.Service;
using Microsoft.Extensions.Caching.Hybrid;

namespace rokis.Providers;

public interface IDurationProvider
{
    Task<TimeSpan> GetDurationAsync();
}

public class SessionDurationProvider : IDurationProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HybridCache _hybridCache;
    private readonly ILogger<SessionDurationProvider> _logger;
    public SessionDurationProvider(IServiceProvider serviceProvider, HybridCache cache, ILogger<SessionDurationProvider> logger)
    {
        _serviceProvider = serviceProvider;
        _hybridCache = cache;
        _logger = logger;
    }
    
    public async Task<TimeSpan> GetDurationAsync()
    {
        _logger.LogInformation("SessionDurationMinutes");
        await using var scope = _serviceProvider.CreateAsyncScope();
        var cfg = scope.ServiceProvider.GetRequiredService<IConfigService>();
        
        var cacheKey = $"{nameof(SessionDurationProvider)}.{nameof(GetDurationAsync)}.SessionDuration";
        var times = await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            await cfg.GetTimesAsync("SessionDuration"));
        return times == null ? TimeSpan.FromMinutes(60) : TimeSpan.FromMinutes(times.Value);
    }
}