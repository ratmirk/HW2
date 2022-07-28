using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RateLimiterCore.Settings;
using Xunit;

namespace RateLimiterCore.Tests;

public class RateLimiterTests
{
    [Fact]
    public async Task RateLimiter_WhenValidRequestCount_ShouldNotBeLimited()
    {
        //Arrange
        var settings = new Mock<ISettings>();
        settings.SetupGet(x => x.LimitPeriod).Returns(TimeSpan.FromSeconds(1));
        settings.SetupGet(x => x.RequestsCount).Returns(1);
        var limiter = new RateLimiter<int>(settings.Object);

        // Act
        var result = await limiter.Invoke(() => Task.FromResult(42), CancellationToken.None);

        // Assert
        result.IsLimited.Should().BeFalse();
    }

    [Fact]
    public async Task RateLimiter_WhenRequestCountMoreLimit_ShouldBeLimited()
    {
        //Arrange
        var settings = new Mock<ISettings>();
        settings.SetupGet(x => x.LimitPeriod).Returns(TimeSpan.FromSeconds(10));
        settings.SetupGet(x => x.RequestsCount).Returns(1);
        var limiter = new RateLimiter<int>(settings.Object);

        // Act
        var resultFalse = await limiter.Invoke(() => Task.FromResult(42), CancellationToken.None);
        var resultTrue = await limiter.Invoke(() => Task.FromResult(42), CancellationToken.None);

        // Assert
        resultFalse.IsLimited.Should().BeFalse();
        resultTrue.IsLimited.Should().BeTrue();
    }

    [Fact]
    public async Task RateLimiter_WhenMultipleRequestsOnDifferentWindows_ShouldBeAllBeExecuted()
    {
        //Arrange
        var settings = new Mock<ISettings>();
        settings.SetupGet(x => x.LimitPeriod).Returns(TimeSpan.FromMilliseconds(50));
        settings.SetupGet(x => x.RequestsCount).Returns(1);
        var limiter = new RateLimiter<int>(settings.Object);

        // Act
        var results = new List<Result<int>>();
        results.Add(await limiter.Invoke(() => Task.FromResult(42), CancellationToken.None));
        results.Add(await limiter.Invoke(() => Task.FromResult(42), CancellationToken.None));
        await Task.Delay(50);
        results.Add(await limiter.Invoke(() => Task.FromResult(42), CancellationToken.None));

        // Assert
        results.First().IsLimited.Should().BeFalse();
        results[1].IsLimited.Should().BeTrue();
        results.Last().IsLimited.Should().BeFalse();
    }
}