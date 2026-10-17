using System.Net;
using System.Text.Json;

namespace VoyageAI.Tests;

public sealed class VoyageClientRequestTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ContextualizedRequest_UsesFlatInputsOnlyForAutoChunking(bool autoChunking)
    {
        var handler = new MockHandler();
        handler.SetResponse(HttpStatusCode.OK, new { data = Array.Empty<object>(), model = "voyage-context-4", usage = new { total_tokens = 0 } });
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/v1/") };
        var request = new ContextualizedEmbeddingRequest
        {
            Inputs = [["first document"], ["second document"]],
            Model = VoyageModels.VoyageContext4,
            InputType = InputType.Document,
            EnableAutoChunking = autoChunking,
            ChunkSize = autoChunking ? 512 : null,
            ChunkOverlap = autoChunking ? 64 : null,
            OutputDimension = 256,
            OutputDtype = "float"
        };

        await new VoyageClient(http).EmbedContextualizedAsync(request, TestContext.Current.CancellationToken);

        using var body = JsonDocument.Parse(Assert.Single(handler.RequestBodies));
        var root = body.RootElement;
        var inputs = root.GetProperty("inputs");
        Assert.Equal(2, inputs.GetArrayLength());
        Assert.Equal(autoChunking ? JsonValueKind.String : JsonValueKind.Array, inputs[0].ValueKind);
        Assert.Equal("first document", (autoChunking ? inputs[0] : inputs[0][0]).GetString());
        Assert.Equal("second document", (autoChunking ? inputs[1] : inputs[1][0]).GetString());
        Assert.Equal("voyage-context-4", root.GetProperty("model").GetString());
        Assert.Equal("document", root.GetProperty("input_type").GetString());
        Assert.Equal(256, root.GetProperty("output_dimension").GetInt32());
        Assert.Equal(autoChunking, root.GetProperty("enable_auto_chunking").GetBoolean());
        if (autoChunking)
        {
            Assert.Equal(512, root.GetProperty("chunk_size").GetInt32());
            Assert.Equal(64, root.GetProperty("chunk_overlap").GetInt32());
        }
        Assert.Equal(new[] { "first document" }, request.Inputs[0]);
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData("query", 1)]
    [InlineData("document", 0)]
    [InlineData("document", 2)]
    public async Task ContextualizedRequest_RejectsInvalidAutoChunkingInputs(string? inputType, int chunkCount)
    {
        var handler = new MockHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/v1/") };
        var request = new ContextualizedEmbeddingRequest
        {
            Inputs = [Enumerable.Repeat("document", chunkCount).ToList()],
            Model = VoyageModels.VoyageContext4,
            InputType = inputType,
            EnableAutoChunking = true
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new VoyageClient(http).EmbedContextualizedAsync(request, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task NullSuccessResponse_ThrowsJsonException()
    {
        var handler = new MockHandler();
        handler.SetRawResponse(HttpStatusCode.OK, new StringContent("null"));
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/v1/") };

        await Assert.ThrowsAsync<JsonException>(() =>
            new VoyageClient(http).EmbedAsync("text", VoyageModels.Voyage4, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("rerank-3")]
    [InlineData("rerank-3-lite")]
    public async Task RerankAsync_SendsNewModelIdentifiers(string model)
    {
        var handler = new MockHandler();
        handler.SetResponse(HttpStatusCode.OK, new { @object = "list", data = Array.Empty<object>(), model, usage = new { total_tokens = 0 } });
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/v1/") };
        var modelConstant = model == "rerank-3" ? VoyageModels.Rerank3 : VoyageModels.Rerank3Lite;

        var response = await new VoyageClient(http).RerankAsync("query", ["document"], modelConstant,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(model, response.Model);
        using var body = JsonDocument.Parse(Assert.Single(handler.RequestBodies));
        Assert.Equal(model, body.RootElement.GetProperty("model").GetString());
    }

    [Fact]
    public async Task PostAsync_PreservesConfiguredHttpVersionAndPolicy()
    {
        var handler = new MockHandler();
        handler.SetResponse(HttpStatusCode.OK, new { @object = "list", data = Array.Empty<object>(), model = "voyage-4", usage = new { total_tokens = 0 } });
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.com/v1/"),
            DefaultRequestVersion = HttpVersion.Version20,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };

        await new VoyageClient(http).EmbedAsync("text", VoyageModels.Voyage4, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpVersion.Version20, handler.LastRequest!.Version);
        Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, handler.LastRequest.VersionPolicy);
    }
}
