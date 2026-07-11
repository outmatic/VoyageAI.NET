using System.Text.Json;

namespace VoyageAI.Tests;

public sealed class SerializationTests
{
    [Fact]
    public void EmbeddingRequest_Serializes_Correctly()
    {
        var request = new EmbeddingRequest
        {
            Input = "hello",
            Model = VoyageModels.Voyage4,
            InputType = InputType.Query,
            Truncation = true,
            OutputDimension = 512,
            OutputDtype = "float",
            EncodingFormat = "base64"
        };

        var json = JsonSerializer.Serialize(request, VoyageJsonContext.Default.EmbeddingRequest);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("hello", root.GetProperty("input").GetString());
        Assert.Equal("voyage-4", root.GetProperty("model").GetString());
        Assert.Equal("query", root.GetProperty("input_type").GetString());
        Assert.True(root.GetProperty("truncation").GetBoolean());
        Assert.Equal(512, root.GetProperty("output_dimension").GetInt32());
        Assert.Equal("float", root.GetProperty("output_dtype").GetString());
        Assert.Equal("base64", root.GetProperty("encoding_format").GetString());
    }

    [Fact]
    public void EmbeddingResponse_Deserializes()
    {
        const string json = """
        {
            "object": "list",
            "data": [{ "object": "embedding", "embedding": [0.1, 0.2], "index": 0 }],
            "model": "voyage-4",
            "usage": { "total_tokens": 5 }
        }
        """;

        var response = JsonSerializer.Deserialize(json, VoyageJsonContext.Default.EmbeddingResponse)!;

        Assert.Equal((string?)"list", (string?)response.Object);
        Assert.Single(response.Data);
        Assert.Equal([0.1f, 0.2f], response.Data[0].Embedding);
        Assert.Equal(0, response.Data[0].Index);
        Assert.Equal((string?)"voyage-4", (string?)response.Model);
        Assert.Equal(5, response.Usage.TotalTokens);
    }

    [Fact]
    public void RerankRequest_RoundTrip()
    {
        var request = new RerankRequest
        {
            Query = "What is AI?",
            Documents = ["doc1", "doc2"],
            Model = VoyageModels.Rerank25,
            TopK = 5,
            ReturnDocuments = true,
            Truncation = false
        };

        var json = JsonSerializer.Serialize(request, VoyageJsonContext.Default.RerankRequest);
        var deserialized = JsonSerializer.Deserialize(json, VoyageJsonContext.Default.RerankRequest)!;

        Assert.Equal((string?)"What is AI?", (string?)deserialized.Query);
        Assert.Equal<List<string>>(["doc1", "doc2"], deserialized.Documents);
        Assert.Equal((string?)VoyageModels.Rerank25, (string?)deserialized.Model);
        Assert.Equal<int?>(5, deserialized.TopK);
        Assert.True((bool?)deserialized.ReturnDocuments);
        Assert.False((bool?)deserialized.Truncation);
    }

    [Fact]
    public void RerankResponse_Deserializes()
    {
        const string json = """
        {
            "object": "list",
            "data": [
                { "index": 1, "relevance_score": 0.95, "document": "doc text" },
                { "index": 0, "relevance_score": 0.3 }
            ],
            "model": "rerank-2.5",
            "usage": { "total_tokens": 20 }
        }
        """;

        var response = JsonSerializer.Deserialize(json, VoyageJsonContext.Default.RerankResponse)!;

        Assert.Equal(2, response.Data.Count);
        Assert.Equal(0.95, response.Data[0].RelevanceScore);
        Assert.Equal((string?)"doc text", (string?)response.Data[0].Document);
        Assert.Null(response.Data[1].Document);
        Assert.Equal(20, response.Usage.TotalTokens);
    }

    [Fact]
    public void MultimodalEmbeddingRequest_Serializes_Correctly()
    {
        var request = new MultimodalEmbeddingRequest
        {
            Inputs =
            [
                new MultimodalInput
                {
                    Content =
                    [
                        new TextContentPart { Text = "a photo" },
                        new ImageUrlContentPart { ImageUrl = "https://example.com/img.png" },
                        new ImageBase64ContentPart { ImageBase64 = "abc123" }
                    ]
                }
            ],
            Model = VoyageModels.VoyageMultimodal35,
            InputType = InputType.Document,
            Truncation = true,
            OutputEncoding = "base64"
        };

        var json = JsonSerializer.Serialize(request, VoyageJsonContext.Default.MultimodalEmbeddingRequest);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("voyage-multimodal-3.5", root.GetProperty("model").GetString());
        Assert.Equal("document", root.GetProperty("input_type").GetString());
        Assert.Equal("base64", root.GetProperty("output_encoding").GetString());

        var inputs = root.GetProperty("inputs");
        Assert.Equal(1, inputs.GetArrayLength());
        var content = inputs[0].GetProperty("content");
        Assert.Equal(3, content.GetArrayLength());
        Assert.Equal("text", content[0].GetProperty("type").GetString());
        Assert.Equal("a photo", content[0].GetProperty("text").GetString());
        Assert.Equal("image_url", content[1].GetProperty("type").GetString());
        Assert.Equal("image_base64", content[2].GetProperty("type").GetString());
    }

    [Fact]
    public void MultimodalEmbeddingResponse_Deserializes()
    {
        const string json = """
        {
            "object": "list",
            "data": [{ "object": "embedding", "embedding": [0.5], "index": 0 }],
            "model": "voyage-multimodal-3.5",
            "usage": { "text_tokens": 3, "image_pixels": 500, "total_tokens": 10 }
        }
        """;

        var response = JsonSerializer.Deserialize(json, VoyageJsonContext.Default.MultimodalEmbeddingResponse)!;

        Assert.Equal((string?)"voyage-multimodal-3.5", (string?)response.Model);
        Assert.Equal(3, response.Usage.TextTokens);
        Assert.Equal(500, response.Usage.ImagePixels);
        Assert.Equal(10, response.Usage.TotalTokens);
    }

    [Fact]
    public void ContextualizedEmbeddingRequest_Serializes_Correctly()
    {
        var request = new ContextualizedEmbeddingRequest
        {
            Inputs = [["chunk 1", "chunk 2"], ["chunk 3"]],
            Model = VoyageModels.VoyageContext4,
            InputType = InputType.Document,
            OutputDimension = 1024,
            OutputDtype = "float",
            EncodingFormat = "base64",
            EnableAutoChunking = true,
            ChunkSize = 512,
            ChunkOverlap = 64
        };

        var json = JsonSerializer.Serialize(request, VoyageJsonContext.Default.ContextualizedEmbeddingRequest);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var inputs = root.GetProperty("inputs");
        Assert.Equal(2, inputs.GetArrayLength());
        Assert.Equal(2, inputs[0].GetArrayLength());
        Assert.Equal("chunk 1", inputs[0][0].GetString());
        Assert.Equal("chunk 3", inputs[1][0].GetString());
        Assert.Equal("voyage-context-4", root.GetProperty("model").GetString());
        Assert.Equal("document", root.GetProperty("input_type").GetString());
        Assert.Equal(1024, root.GetProperty("output_dimension").GetInt32());
        Assert.Equal("float", root.GetProperty("output_dtype").GetString());
        Assert.Equal("base64", root.GetProperty("encoding_format").GetString());
        Assert.True(root.GetProperty("enable_auto_chunking").GetBoolean());
        Assert.Equal(512, root.GetProperty("chunk_size").GetInt32());
        Assert.Equal(64, root.GetProperty("chunk_overlap").GetInt32());
    }

    [Fact]
    public void ContextualizedEmbeddingRequest_NullableFields_OmittedWhenNull()
    {
        var request = new ContextualizedEmbeddingRequest
        {
            Inputs = [["chunk"]],
            Model = VoyageModels.VoyageContext4
        };

        var json = JsonSerializer.Serialize(request, VoyageJsonContext.Default.ContextualizedEmbeddingRequest);

        Assert.DoesNotContain("input_type", json);
        Assert.DoesNotContain("output_dimension", json);
        Assert.DoesNotContain("output_dtype", json);
        Assert.DoesNotContain("encoding_format", json);
        Assert.DoesNotContain("enable_auto_chunking", json);
        Assert.DoesNotContain("chunk_size", json);
        Assert.DoesNotContain("chunk_overlap", json);
    }

    [Fact]
    public void ContextualizedEmbeddingResponse_Deserializes()
    {
        const string json = """
        {
            "object": "list",
            "data": [
                {
                    "object": "list",
                    "data": [
                        { "object": "embedding", "embedding": [0.1, 0.2], "index": 0, "text": "chunk 1" },
                        { "object": "embedding", "embedding": [0.3, 0.4], "index": 1, "text": "chunk 2" }
                    ],
                    "index": 0
                },
                {
                    "object": "list",
                    "data": [
                        { "object": "embedding", "embedding": [0.5, 0.6], "index": 0 }
                    ],
                    "index": 1
                }
            ],
            "model": "voyage-context-4",
            "usage": { "total_tokens": 30 },
            "chunker_version": "1.0.0"
        }
        """;

        var response = JsonSerializer.Deserialize(json, VoyageJsonContext.Default.ContextualizedEmbeddingResponse)!;

        Assert.Equal(2, response.Data.Count);
        Assert.Equal(0, response.Data[0].Index);
        Assert.Equal(2, response.Data[0].Data.Count);
        Assert.Equal([0.1f, 0.2f], response.Data[0].Data[0].Embedding);
        Assert.Equal((string?)"chunk 1", (string?)response.Data[0].Data[0].Text);
        Assert.Equal(1, response.Data[0].Data[1].Index);
        Assert.Equal(1, response.Data[1].Index);
        Assert.Null(response.Data[1].Data[0].Text);
        Assert.Equal((string?)"voyage-context-4", (string?)response.Model);
        Assert.Equal(30, response.Usage.TotalTokens);
        Assert.Equal((string?)"1.0.0", (string?)response.ChunkerVersion);
    }

    [Fact]
    public void VoyageApiError_Deserializes()
    {
        const string json = """{"detail":"bad request"}""";

        var error = JsonSerializer.Deserialize(json, VoyageJsonContext.Default.VoyageApiError)!;

        Assert.Equal((string?)"bad request", (string?)error.Detail);
    }

    [Fact]
    public void EmbeddingRequest_NullableFields_OmittedWhenNull()
    {
        var request = new EmbeddingRequest { Input = "test", Model = "voyage-4" };
        var json = JsonSerializer.Serialize(request, VoyageJsonContext.Default.EmbeddingRequest);

        Assert.DoesNotContain("input_type", json);
        Assert.DoesNotContain("truncation", json);
        Assert.DoesNotContain("output_dimension", json);
        Assert.DoesNotContain("output_dtype", json);
        Assert.DoesNotContain("encoding_format", json);
    }
}
