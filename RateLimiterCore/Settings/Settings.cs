using Microsoft.Extensions.Configuration;

namespace RateLimiterCore.Settings;

public class Settings : ISettings
{
    private readonly IConfiguration _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

    public int RequestsCount => Convert.ToInt32(_configuration["RequestsCount"]);

    public TimeSpan LimitPeriod => TimeSpan.FromSeconds(Convert.ToDouble(_configuration["LimitPeriod"]));
}