using MassTransitDemo.ApiService.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace MassTransitDemo.Tests;

public class DebounceGuardTests
{
    [Fact]
    public void Should_Acquire_On_First_Request_And_Reject_Duplicate_Within_Interval()
    {
        // Arrange
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var guard = new DebounceGuard(memoryCache);
        var interval = TimeSpan.FromMilliseconds(500);
        var key = "test-customer-001";

        // Act & Assert
        var firstAttempt = guard.TryAcquire(key, interval);
        var secondAttempt = guard.TryAcquire(key, interval);

        Assert.True(firstAttempt, "First attempt should acquire lock.");
        Assert.False(secondAttempt, "Immediate second attempt should be debounced.");
    }

    [Fact]
    public async Task Should_Allow_Request_After_Interval_Expires()
    {
        // Arrange
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var guard = new DebounceGuard(memoryCache);
        var interval = TimeSpan.FromMilliseconds(50);
        var key = "test-customer-002";

        // Act
        var firstAttempt = guard.TryAcquire(key, interval);
        await Task.Delay(100); // Wait for cache entry to expire
        var attemptAfterExpiry = guard.TryAcquire(key, interval);

        // Assert
        Assert.True(firstAttempt);
        Assert.True(attemptAfterExpiry, "Attempt after interval expiration should succeed.");
    }
}

