using System.Net;
using System.Text.Json;

namespace VoyageAI.Tests;

public sealed class BatchingTests
{
    [Fact]
    public async Task BatchAsync_StreamsInOrderAndRefillsBeforeSlowerRequestsComplete()
    {
        var releaseSecond = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thirdStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new MockHandler
        {
            ResponseFactory = async (body, ct) =>
            {
                var index = ReadInputIndex(body);
                if (index == 1)
                    await releaseSecond.Task.WaitAsync(ct);
                if (index == 2)
                    thirdStarted.TrySetResult();
                return Response(index);
            }
        };
        using var http = CreateHttpClient(handler);
        await using var batches = new VoyageClient(http).EmbedBatchAsync(
            ["0", "1", "2"], VoyageModels.Voyage4, batchSize: 1, maxConcurrency: 2,
            cancellationToken: TestContext.Current.CancellationToken).GetAsyncEnumerator(TestContext.Current.CancellationToken);

        Assert.True(await batches.MoveNextAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        Assert.Equal("batch-0", batches.Current.Model);
        Assert.Equal(2, handler.CallCount);

        var second = batches.MoveNextAsync().AsTask();
        await thirdStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.False(second.IsCompleted);
        releaseSecond.TrySetResult();
        Assert.True(await second);
        Assert.Equal("batch-1", batches.Current.Model);

        Assert.True(await batches.MoveNextAsync());
        Assert.Equal("batch-2", batches.Current.Model);
        Assert.False(await batches.MoveNextAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BatchAsync_CancelsOutstandingRequestsOnEarlyStopOrFailure(bool fail)
    {
        var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new MockHandler
        {
            ResponseFactory = async (body, ct) =>
            {
                if (ReadInputIndex(body) == 0)
                {
                    await secondStarted.Task.WaitAsync(ct);
                    return fail
                        ? new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("{\"detail\":\"failed batch\"}") }
                        : Response(0);
                }

                secondStarted.TrySetResult();
                try
                {
                    await Task.Delay(Timeout.Infinite, ct);
                    return Response(1);
                }
                finally
                {
                    secondStopped.TrySetResult();
                }
            }
        };
        using var http = CreateHttpClient(handler);
        var client = new VoyageClient(http);

        async Task ConsumeFirstAsync()
        {
            await foreach (var _ in client.EmbedBatchAsync(
                ["0", "1", "2"], VoyageModels.Voyage4, batchSize: 1, maxConcurrency: 2,
                cancellationToken: TestContext.Current.CancellationToken))
            {
                break;
            }
        }

        var consumption = ConsumeFirstAsync().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        if (fail)
        {
            var error = await Assert.ThrowsAsync<VoyageApiException>(() => consumption);
            Assert.Equal("failed batch", error.Detail);
        }
        else
        {
            await consumption;
        }

        Assert.True(secondStopped.Task.IsCompletedSuccessfully);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task BatchAsync_DoesNotEnumerateCanceledInputs()
    {
        var handler = new MockHandler();
        using var http = CreateHttpClient(handler);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var inputs = Enumerable.Range(0, 1).Select<int, string>(_ =>
            throw new InvalidOperationException("Canceled inputs should not be enumerated."));

        await using var batches = new VoyageClient(http).EmbedBatchAsync(inputs, VoyageModels.Voyage4,
                cancellationToken: TestContext.Current.CancellationToken)
            .GetAsyncEnumerator(cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => batches.MoveNextAsync().AsTask());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task BatchAsync_CancellationBetweenResultsStopsFurtherRequests()
    {
        var handler = new MockHandler { ResponseFactory = (body, _) => Task.FromResult(Response(ReadInputIndex(body))) };
        using var http = CreateHttpClient(handler);
        using var cancellation = new CancellationTokenSource();
        await using var batches = new VoyageClient(http).EmbedBatchAsync(
            ["0", "1", "2"], VoyageModels.Voyage4, batchSize: 1, maxConcurrency: 2,
            cancellationToken: cancellation.Token).GetAsyncEnumerator(TestContext.Current.CancellationToken);

        Assert.True(await batches.MoveNextAsync());
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => batches.MoveNextAsync().AsTask());
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void BatchMethods_ValidateLimitsBeforeEnumeration(int batchSize, int concurrency)
    {
        using var http = CreateHttpClient(new MockHandler());
        var client = new VoyageClient(http);

        Assert.Throws<ArgumentOutOfRangeException>(() => client.EmbedBatchAsync(
            [], VoyageModels.Voyage4, batchSize: batchSize, maxConcurrency: concurrency,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentOutOfRangeException>(() => client.EmbedContextualizedBatchAsync(
            [], VoyageModels.VoyageContext4, batchSize: batchSize, maxConcurrency: concurrency,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentOutOfRangeException>(() => client.EmbedMultimodalBatchAsync(
            [], VoyageModels.VoyageMultimodal35, batchSize: batchSize, maxConcurrency: concurrency,
            cancellationToken: TestContext.Current.CancellationToken));
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler)
        => new(handler) { BaseAddress = new Uri("https://example.com/v1/") };

    private static int ReadInputIndex(string body)
    {
        using var document = JsonDocument.Parse(body);
        return int.Parse(document.RootElement.GetProperty("input")[0].GetString()!, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static HttpResponseMessage Response(int index) => new(HttpStatusCode.OK)
    {
        Content = new StringContent($$$"""
            {"object":"list","data":[],"model":"batch-{{{index}}}","usage":{"total_tokens":0}}
            """)
    };
}
