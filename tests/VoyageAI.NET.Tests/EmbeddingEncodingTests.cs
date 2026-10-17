using System.Net;
using System.Text.Json;

namespace VoyageAI.Tests;

public sealed class EmbeddingEncodingTests
{
    [Theory]
    [InlineData("array")]
    [InlineData("list")]
    [InlineData("iterator")]
    public void EmbeddingRequest_SerializesStringSequencesWithoutReflection(string inputKind)
    {
        object input = inputKind switch
        {
            "array" => new[] { "hello", "world" },
            "list" => new List<string> { "hello", "world" },
            _ => new[] { "hello", "world" }.Select(text => text)
        };
        var request = new EmbeddingRequest { Input = input, Model = VoyageModels.VoyageCode4 };

        var json = JsonSerializer.Serialize(request, VoyageJsonContext.Default.EmbeddingRequest);
        using var document = JsonDocument.Parse(json);
        Assert.Equal("voyage-code-4", document.RootElement.GetProperty("model").GetString());
        Assert.Equal(new[] { "hello", "world" }, document.RootElement.GetProperty("input")
            .EnumerateArray().Select(item => item.GetString()));

        var roundTrip = JsonSerializer.Deserialize(json, VoyageJsonContext.Default.EmbeddingRequest)!;
        Assert.Equal(new[] { "hello", "world" }, Assert.IsType<List<string>>(roundTrip.Input));
    }

    [Fact]
    public void EmbeddingRequest_RejectsUnsupportedInputTypes()
    {
        var request = new EmbeddingRequest { Input = 42, Model = VoyageModels.Voyage4 };

        Assert.Throws<JsonException>(() => JsonSerializer.Serialize(request, VoyageJsonContext.Default.EmbeddingRequest));
    }

    [Theory]
    [InlineData("text", "float")]
    [InlineData("text", "int8")]
    [InlineData("text", "uint8")]
    [InlineData("text", "binary")]
    [InlineData("text", "ubinary")]
    [InlineData("contextualized", "float")]
    [InlineData("contextualized", "int8")]
    [InlineData("contextualized", "uint8")]
    [InlineData("contextualized", "binary")]
    [InlineData("contextualized", "ubinary")]
    [InlineData("multimodal", "float")]
    [InlineData("multimodal", "int8")]
    [InlineData("multimodal", "uint8")]
    [InlineData("multimodal", "binary")]
    [InlineData("multimodal", "ubinary")]
    public async Task EmbeddingEndpoints_DecodeBase64UsingRequestedDtype(string endpoint, string dtype)
    {
        var base64 = dtype == "float" ? "AACAPwAAAMA=" : "/wB/";
        var item = $$"""{"object":"embedding","embedding":"{{base64}}","index":0}""";
        if (endpoint == "contextualized")
            item = $$"""{"data":[{{item}}],"index":0}""";

        var json = $$$"""
            {"object":"list","data":[{{{item}}}],"model":"test-model",
             "usage":{"total_tokens":2,"text_tokens":2,"image_pixels":0}}
            """;
        var handler = new MockHandler();
        handler.SetRawResponse(HttpStatusCode.OK, new StringContent(json));
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/v1/") };
        var client = new VoyageClient(http);
        var cancellationToken = TestContext.Current.CancellationToken;

        var embedding = endpoint switch
        {
            "text" => (await client.EmbedAsync(new EmbeddingRequest
            {
                Input = "text", Model = VoyageModels.VoyageCode4, OutputDtype = dtype, EncodingFormat = "base64"
            }, cancellationToken)).Data[0].Embedding,
            "contextualized" => (await client.EmbedContextualizedAsync(new ContextualizedEmbeddingRequest
            {
                Inputs = [["text"]], Model = VoyageModels.VoyageContext4, OutputDtype = dtype, EncodingFormat = "base64"
            }, cancellationToken)).Data[0].Data[0].Embedding,
            _ => (await client.EmbedMultimodalAsync(new MultimodalEmbeddingRequest
            {
                Inputs = [new MultimodalInput { Content = [new TextContentPart { Text = "text" }] }],
                Model = VoyageModels.VoyageMultimodal35, OutputDtype = dtype, OutputEncoding = "base64"
            }, cancellationToken)).Data[0].Embedding
        };

        float[] expected = dtype switch
        {
            "float" => [1, -2],
            "int8" or "binary" => [-1, 0, 127],
            _ => [255, 0, 127]
        };
        Assert.Equal(expected, embedding);
        using var request = JsonDocument.Parse(Assert.Single(handler.RequestBodies));
        Assert.Equal(dtype, request.RootElement.GetProperty("output_dtype").GetString());
        var encodingName = endpoint == "multimodal" ? "output_encoding" : "encoding_format";
        Assert.Equal("base64", request.RootElement.GetProperty(encodingName).GetString());
    }

    [Theory]
    [InlineData("\"AQ==\"")]
    [InlineData("\"not base64!\"")]
    [InlineData("[\"not a number\"]")]
    [InlineData("{}")]
    public void EmbeddingResponse_RejectsMalformedVectors(string embedding)
    {
        var json = $$$"""
            {"object":"list","data":[{"object":"embedding","embedding":{{{embedding}}},"index":0}],
             "model":"voyage-4","usage":{"total_tokens":1}}
            """;

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(json, VoyageJsonContext.Default.EmbeddingResponse));
    }
}
