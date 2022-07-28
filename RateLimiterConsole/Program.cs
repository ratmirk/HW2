// See https://aka.ms/new-console-template for more information

using RateLimiterCore;
using RateLimiterCore.Settings;

namespace RateLimiterConsole;

public static class Program
{
    private static readonly ISettings Settings = new Settings();

    public static async Task Main(string[] args)
    {
        var random = new Random();

        var limiter = new RateLimiter<int>(Settings);

        var tasksList = new List<Task<Result<int>>>();

        for (int i = 0; i < 50; i++)
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(random.Next(100, 5000)));

                return await limiter.Invoke(async () =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(250));

                        return random.Next();
                    },
                    CancellationToken.None);
            });

            tasksList.Add(task);
        }

        await Task.WhenAll(tasksList);

        foreach (var task in tasksList)
        {
            Console.WriteLine(task.Result.Value + " isLimited:" + task.Result.IsLimited);
        }
    }
}