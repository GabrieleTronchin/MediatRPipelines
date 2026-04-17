using MediatR.Playground.Model.Queries.Entity;
using MediatR.Playground.Pipelines.Query;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ZiggyCreatures.Caching.Fusion;

namespace MediatR.Playground.Tests;

public class CachingBehaviorTests
{
    private readonly GetAllSampleEntitiesQuery _query = new();

    private readonly IEnumerable<GetAllSampleEntitiesQueryResult> _expectedResponse = new[]
    {
        new GetAllSampleEntitiesQueryResult
        {
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            Description = "Test entity"
        }
    };

    /// <summary>
    /// Validates: Requirements 4.1
    /// WHEN a query is processed by CachingBehavior and the result is not in cache (cache miss),
    /// CachingBehavior invokes the next() delegate and returns the response.
    /// </summary>
    [Fact]
    public async Task Handle_OnCacheMiss_InvokesNextAndReturnsResponse()
    {
        // Arrange
        var logger = Substitute.For<ILogger<CachingBehavior<GetAllSampleEntitiesQuery, IEnumerable<GetAllSampleEntitiesQueryResult>>>>();
        var cache = Substitute.For<IFusionCache>();
        var next = Substitute.For<RequestHandlerDelegate<IEnumerable<GetAllSampleEntitiesQueryResult>>>();

        next.Invoke().Returns(_expectedResponse);

        // The extension method GetOrSetAsync(key, defaultValue, Action<opts>, token)
        // internally calls cache.DefaultEntryOptionsProvider and cache.DefaultEntryOptions
        // before delegating to the interface method. Set these up so the extension method works.
        cache.DefaultEntryOptions.Returns(new FusionCacheEntryOptions());
        cache.DefaultEntryOptionsProvider.Returns((FusionCacheEntryOptionsProvider?)null);

        // Mock the actual interface method that the extension method delegates to.
        // Signature: GetOrSetAsync<TValue>(string key, TValue defaultValue, FusionCacheEntryOptions? options, IEnumerable<string>? tags, CancellationToken token)
        cache.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<GetAllSampleEntitiesQueryResult>>(),
            Arg.Any<FusionCacheEntryOptions?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<CancellationToken>()
        ).Returns(callInfo => new ValueTask<IEnumerable<GetAllSampleEntitiesQueryResult>>(
            callInfo.ArgAt<IEnumerable<GetAllSampleEntitiesQueryResult>>(1)));

        var behavior = new CachingBehavior<GetAllSampleEntitiesQuery, IEnumerable<GetAllSampleEntitiesQueryResult>>(logger, cache);

        // Act
        var result = await behavior.Handle(_query, next, CancellationToken.None);

        // Assert
        Assert.Equal(_expectedResponse, result);
        await next.Received(1).Invoke();
    }

    /// <summary>
    /// Validates: Requirements 4.2
    /// WHEN a query is processed by CachingBehavior,
    /// CachingBehavior uses the request's CacheKey to interact with FusionCache.
    /// </summary>
    [Fact]
    public async Task Handle_UsesCacheKeyFromRequest()
    {
        // Arrange
        var logger = Substitute.For<ILogger<CachingBehavior<GetAllSampleEntitiesQuery, IEnumerable<GetAllSampleEntitiesQueryResult>>>>();
        var cache = Substitute.For<IFusionCache>();
        var next = Substitute.For<RequestHandlerDelegate<IEnumerable<GetAllSampleEntitiesQueryResult>>>();

        next.Invoke().Returns(_expectedResponse);

        cache.DefaultEntryOptions.Returns(new FusionCacheEntryOptions());
        cache.DefaultEntryOptionsProvider.Returns((FusionCacheEntryOptionsProvider?)null);

        cache.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<GetAllSampleEntitiesQueryResult>>(),
            Arg.Any<FusionCacheEntryOptions?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<CancellationToken>()
        ).Returns(callInfo => new ValueTask<IEnumerable<GetAllSampleEntitiesQueryResult>>(
            callInfo.ArgAt<IEnumerable<GetAllSampleEntitiesQueryResult>>(1)));

        var behavior = new CachingBehavior<GetAllSampleEntitiesQuery, IEnumerable<GetAllSampleEntitiesQueryResult>>(logger, cache);

        // Act
        await behavior.Handle(_query, next, CancellationToken.None);

        // Assert — verify GetOrSetAsync was called with the expected cache key
        await cache.Received(1).GetOrSetAsync(
            "GetAllSampleEntitiesQuery-ALL",
            Arg.Any<IEnumerable<GetAllSampleEntitiesQueryResult>>(),
            Arg.Any<FusionCacheEntryOptions?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<CancellationToken>()
        );
    }
}
