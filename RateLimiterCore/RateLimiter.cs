using RateLimiterCore.Settings;

namespace RateLimiterCore;

public class RateLimiter<T> : IRateLimiter<T>, IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly ISettings _settings;
    private DateTime _currentWindowStart;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly object _lock = new();

    public RateLimiter(ISettings settings)
    {
        _settings = settings;
        _semaphoreSlim = new SemaphoreSlim(settings.RequestsCount);
        _currentWindowStart = DateTime.UtcNow;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task<Result<T>> Invoke(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            if (now - _currentWindowStart > _settings.LimitPeriod)
            {
                _currentWindowStart = now;
                _semaphoreSlim.Release(_settings.RequestsCount);
            }

            if (!_semaphoreSlim.Wait(0, cancellationToken))
            {
                Console.WriteLine(
                    $"Fail {now:hh.mm.ss.ffffff} currentWindowStart: {_currentWindowStart:hh.mm.ss.ffffff}");
                return Result<T>.Fail();
            }

            Console.WriteLine(
                $"Success {now:hh.mm.ss.ffffff} currentWindowStart: {_currentWindowStart:hh.mm.ss.ffffff}");
        }

        return Result<T>.Success(await action.Invoke());
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _semaphoreSlim.Dispose();
        GC.SuppressFinalize(this);
    }
}