namespace RateLimiterCore.Settings;

public interface ISettings
{
    public int RequestsCount { get; }

    public TimeSpan LimitPeriod { get; }
}