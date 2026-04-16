using System.Net;
using System.Net.Http.Json;

namespace VoyageAI.Tests;

public sealed class VoyageClientTests
{
    private static (VoyageClient client, MockHandler handler) CreateClient()
    {
        var handler = new MockHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.voyageai.com/v1/") };
        return (new VoyageClient(httpClient), handler);
    }

    [Fact]
    public async Task EmbedAsync_SendsCorrectRequest_And_DeserializesResponse()
    {
        var (client, handler) = CreateClient();

        var responseBody = new
        {
            @object = "list",
            data = new[] { new { @object = "embedding", embedding = new[] { 0.1, 0.2, 0.3 }, index = 0 } },
            model = "voyage-4",
            usage = new { total_tokens = 10 }
        };
        handler.SetResponse(HttpStatusCode.OK, responseBody);

        var result = await client.EmbedAsync(new EmbeddingRequest
        {
            Input = "hello world",
            Model = VoyageModels.Voyage4,
            InputType = InputType.Query
        }, TestContext.Current.CancellationToken);

        Assert.Equal("list", result.Object);
        Assert.Single(result.Data);
        Assert.Equal([0.1f, 0.2f, 0.3f], result.Data[0].Embedding);
        Assert.Equal(10, result.Usage.TotalTokens);

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("https://api.voyageai.com/v1/embeddings", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task EmbedMultimodalAsync_SendsCorrectRequest()
    {
        var (client, handler) = CreateClient();

        var responseBody = new
        {
            @object = "list",
            data = new[] { new { @object = "embedding", embedding = new[] { 0.5 }, index = 0 } },
            model = "voyage-multimodal-3.5",
            usage = new { text_tokens = 5, image_pixels = 1000, total_tokens = 15 }
        };
        handler.SetResponse(HttpStatusCode.OK, responseBody);

        var result = await client.EmbedMultimodalAsync(new MultimodalEmbeddingRequest
        {
            Inputs =
            [
                new MultimodalInput
                {
                    Content = [new TextContentPart { Text = "a cat" }]
                }
            ],
            Model = VoyageModels.VoyageMultimodal35
        }, TestContext.Current.CancellationToken);

        Assert.Equal("voyage-multimodal-3.5", result.Model);
        Assert.Equal(5, result.Usage.TextTokens);
        Assert.Equal(1000, result.Usage.ImagePixels);
    }

    [Fact]
    public async Task RerankAsync_SendsCorrectRequest()
    {
        var (client, handler) = CreateClient();

        var responseBody = new
        {
            @object = "list",
            data = new[] { new { index = 1, relevance_score = 0.95 }, new { index = 0, relevance_score = 0.3 } },
            model = "rerank-2.5",
            usage = new { total_tokens = 20 }
        };
        handler.SetResponse(HttpStatusCode.OK, responseBody);

        var result = await client.RerankAsync(new RerankRequest
        {
            Query = "What is AI?",
            Documents = ["Artificial intelligence is...", "The weather today is..."],
            Model = VoyageModels.Rerank25,
            TopK = 2
        }, TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Data.Count);
        Assert.Equal(0.95, result.Data[0].RelevanceScore);
        Assert.Equal(1, result.Data[0].Index);
    }

    [Fact]
    public async Task EmbedAsync_StringOverload_Works()
    {
        var (client, handler) = CreateClient();

        var responseBody = new
        {
            @object = "list",
            data = new[] { new { @object = "embedding", embedding = new[] { 0.1, 0.2 }, index = 0 } },
            model = "voyage-4",
            usage = new { total_tokens = 5 }
        };
        handler.SetResponse(HttpStatusCode.OK, responseBody);

        var result = await client.EmbedAsync("hello", VoyageModels.Voyage4, InputType.Query, TestContext.Current.CancellationToken);

        Assert.Equal([0.1f, 0.2f], result.Data[0].Embedding);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
    }

    [Fact]
    public async Task EmbedAsync_EnumerableOverload_Works()
    {
        var (client, handler) = CreateClient();

        var responseBody = new
        {
            @object = "list",
            data = new[]
            {
                new { @object = "embedding", embedding = new[] { 0.1 }, index = 0 },
                new { @object = "embedding", embedding = new[] { 0.2 }, index = 1 }
            },
            model = "voyage-4",
            usage = new { total_tokens = 10 }
        };
        handler.SetResponse(HttpStatusCode.OK, responseBody);

        var result = await client.EmbedAsync(
            new[] { "hello", "world" }, VoyageModels.Voyage4, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task RerankAsync_SimplifiedOverload_Works()
    {
        var (client, handler) = CreateClient();

        var responseBody = new
        {
            @object = "list",
            data = new[] { new { index = 0, relevance_score = 0.9 } },
            model = "rerank-2.5",
            usage = new { total_tokens = 10 }
        };
        handler.SetResponse(HttpStatusCode.OK, responseBody);

        var result = await client.RerankAsync(
            "What is AI?", ["doc1", "doc2"], VoyageModels.Rerank25, topK: 1, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Single(result.Data);
        Assert.Equal(0.9, result.Data[0].RelevanceScore);
    }

    [Fact]
    public async Task EmbedBatchAsync_SplitsIntoBatches_And_PreservesOrder()
    {
        var (client, handler) = CreateClient();

        var responseBody = new
        {
            @object = "list",
            data = new[] { new { @object = "embedding", embedding = new[] { 0.1 }, index = 0 } },
            model = "voyage-4",
            usage = new { total_tokens = 1 }
        };
        handler.SetResponse(HttpStatusCode.OK, responseBody);

        // 5 inputs with batchSize=2 → 3 batches
        var inputs = Enumerable.Range(0, 5).Select(i => $"text-{i}");
        var responses = new List<EmbeddingResponse>();

        await foreach (var response in client.EmbedBatchAsync(inputs, VoyageModels.Voyage4, batchSize: 2, maxConcurrency: 2, cancellationToken: TestContext.Current.CancellationToken))
        {
            responses.Add(response);
        }

        Assert.Equal(3, responses.Count);
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task EmbedMultimodalBatchAsync_SplitsIntoBatches()
    {
        var (client, handler) = CreateClient();

        var responseBody = new
        {
            @object = "list",
            data = new[] { new { @object = "embedding", embedding = new[] { 0.1 }, index = 0 } },
            model = "voyage-multimodal-3.5",
            usage = new { text_tokens = 1, image_pixels = 0, total_tokens = 1 }
        };
        handler.SetResponse(HttpStatusCode.OK, responseBody);

        var inputs = Enumerable.Range(0, 5).Select(i => new MultimodalInput
        {
            Content = [new TextContentPart { Text = $"text-{i}" }]
        });
        var responses = new List<MultimodalEmbeddingResponse>();

        await foreach (var response in client.EmbedMultimodalBatchAsync(inputs, VoyageModels.VoyageMultimodal35, batchSize: 2, maxConcurrency: 2, cancellationToken: TestContext.Current.CancellationToken))
        {
            responses.Add(response);
        }

        Assert.Equal(3, responses.Count);
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task ApiError_ThrowsVoyageApiException()
    {
        var (client, handler) = CreateClient();

        handler.SetResponse(HttpStatusCode.BadRequest, new { detail = "Invalid model name" });

        var ex = await Assert.ThrowsAsync<VoyageApiException>(() =>
            client.EmbedAsync(new EmbeddingRequest { Input = "test", Model = "bad-model" }, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("Invalid model name", ex.Detail);
    }

    [Fact]
    public async Task ApiError_NonJsonBody_StillThrowsVoyageApiException()
    {
        var (client, handler) = CreateClient();
        handler.SetRawResponse(HttpStatusCode.BadGateway,
            new StringContent("<html>502 Bad Gateway</html>", System.Text.Encoding.UTF8, "text/html"));

        var ex = await Assert.ThrowsAsync<VoyageApiException>(() =>
            client.EmbedAsync("test", VoyageModels.Voyage4, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Null(ex.Detail);
    }

    [Fact]
    public async Task EmbedAsync_CancellationToken_CancelsInFlightRequest()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, new { });
        handler.BeforeResponse = ct => Task.Delay(Timeout.Infinite, ct);

        using var cts = new CancellationTokenSource();
        var task = client.EmbedAsync("test", VoyageModels.Voyage4, cancellationToken: cts.Token);
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task EmbedBatchAsync_EmptyInput_ProducesNoRequests()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, EmptyEmbeddingResponse());

        var produced = 0;
        await foreach (var _ in client.EmbedBatchAsync(
            Array.Empty<string>(), VoyageModels.Voyage4,
            cancellationToken: TestContext.Current.CancellationToken))
        {
            produced++;
        }

        Assert.Equal(0, produced);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task EmbedBatchAsync_SplitsAtExactBatchBoundary()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, EmptyEmbeddingResponse());

        var inputs = Enumerable.Range(0, 256).Select(i => $"t{i}").ToArray();
        await foreach (var _ in client.EmbedBatchAsync(
            inputs, VoyageModels.Voyage4, batchSize: 128, maxConcurrency: 4,
            cancellationToken: TestContext.Current.CancellationToken)) { }

        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task EmbedBatchAsync_RespectsMaxConcurrency()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, EmptyEmbeddingResponse());
        handler.BeforeResponse = ct => Task.Delay(50, ct);

        var inputs = Enumerable.Range(0, 20).Select(i => $"t{i}").ToArray();
        await foreach (var _ in client.EmbedBatchAsync(
            inputs, VoyageModels.Voyage4, batchSize: 1, maxConcurrency: 4,
            cancellationToken: TestContext.Current.CancellationToken)) { }

        Assert.Equal(20, handler.CallCount);
        Assert.True(handler.MaxConcurrent <= 4, $"Expected <=4, observed {handler.MaxConcurrent}");
        Assert.True(handler.MaxConcurrent >= 2, $"Expected parallelism >=2, observed {handler.MaxConcurrent}");
    }

    private static object EmptyEmbeddingResponse() => new
    {
        @object = "list",
        data = Array.Empty<object>(),
        model = "voyage-4",
        usage = new { total_tokens = 0 }
    };
}

internal sealed class MockHandler : HttpMessageHandler
{
    private HttpStatusCode _statusCode;
    private object? _responseBody;
    private HttpContent? _rawContent;
    private readonly object _lock = new();
    private int _currentConcurrent;

    public HttpRequestMessage? LastRequest { get; private set; }
    public int CallCount { get; private set; }
    public List<HttpRequestMessage> Requests { get; } = new();
    public int MaxConcurrent { get; private set; }

    /// <summary>Optional async hook run before producing the response. Useful for testing cancellation and concurrency.</summary>
    public Func<CancellationToken, Task>? BeforeResponse { get; set; }

    public void SetResponse(HttpStatusCode statusCode, object responseBody)
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
        _rawContent = null;
    }

    public void SetRawResponse(HttpStatusCode statusCode, HttpContent content)
    {
        _statusCode = statusCode;
        _rawContent = content;
        _responseBody = null;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            LastRequest = request;
            Requests.Add(request);
            CallCount++;
            _currentConcurrent++;
            if (_currentConcurrent > MaxConcurrent) MaxConcurrent = _currentConcurrent;
        }
        try
        {
            if (BeforeResponse is not null)
                await BeforeResponse(cancellationToken);

            return new HttpResponseMessage(_statusCode)
            {
                Content = _rawContent ?? JsonContent.Create(_responseBody)
            };
        }
        finally
        {
            lock (_lock) _currentConcurrent--;
        }
    }
}
