using idcc.Context;
using idcc.Infrastructures;
using idcc.Providers;
using Microsoft.EntityFrameworkCore;

namespace idcc.Service;

public class SessionTimeoutWorker  : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDurationProvider  _durationProvider;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);
    private readonly ILogger<SessionTimeoutWorker> _logger;

    public SessionTimeoutWorker(IServiceProvider serviceProvider, IDurationProvider durationProvider, ILogger<SessionTimeoutWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _durationProvider = durationProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<IdccContext>();
                var duration = await _durationProvider.GetDurationAsync();

                var now = DateTime.UtcNow;

                var expired = await context.Sessions
                    .Where(s => s.EndTime == null && now - s.StartTime > duration)
                    .Include(session => session.Token)
                    .ToListAsync(cancellationToken: cancellationToken);
                if (expired.Any())
                {
                    foreach (var session in expired)
                    {
                        session.EndTime = now;
                        session.Score = 0;
                        session.Token.Status = TokenStatus.Used;
                        session.Token.Score = 0;
                    }

                    await context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Closed {Count} expired sessions", expired.Count);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SessionTimeoutWorker error");
            }

            await Task.Delay(_interval, cancellationToken);
        }
    }
}