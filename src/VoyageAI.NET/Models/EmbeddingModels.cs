using System.Text.Json.Serialization;

namespace VoyageAI.Models;

/// <summary>
/// Request body for the text embeddings endpoint.
/// </summary>
public sealed record EmbeddingRequest
{
    /// <summary>
    /// The input to embed — a single string or a list of strings.
    /// </summary>
    public required object Input { get; init; }

    /// <summary>
    /// The model to use for embedding (e.g. <see cref="VoyageModels.Voyage4"/>).
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Optional input type hint: <see cref="InputType.Query"/> or <see cref="InputType.Document"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InputType { get; init; }

    /// <summary>
    /// Whether to truncate inputs that exceed the model's context length. Defaults to <c>true</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Truncation { get; init; }

    /// <summary>
    /// Desired output embedding dimension. Only supported by certain models.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? OutputDimension { get; init; }

    /// <summary>
    /// Output data type: <c>"float"</c>, <c>"int8"</c>, <c>"uint8"</c>, <c>"binary"</c>, or <c>"ubinary"</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OutputDtype { get; init; }

    /// <summary>
    /// Encoding format for the response: <c>null</c> (default JSON arrays) or <c>"base64"</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EncodingFormat { get; init; }
}

/// <summary>
/// Response from the text embeddings endpoint.
/// </summary>
public sealed record EmbeddingResponse
{
    /// <summary>
    /// Object type — always <c>"list"</c>.
    /// </summary>
    public required string Object { get; init; }

    /// <summary>
    /// The list of embedding results.
    /// </summary>
    public required List<EmbeddingData> Data { get; init; }

    /// <summary>
    /// The model used to generate the embeddings.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Token usage information.
    /// </summary>
    public required EmbeddingUsage Usage { get; init; }
}

/// <summary>
/// A single embedding result.
/// </summary>
public sealed record EmbeddingData
{
    /// <summary>
    /// Object type — always <c>"embedding"</c>.
    /// </summary>
    public required string Object { get; init; }

    /// <summary>
    /// The embedding vector.
    /// </summary>
    public required float[] Embedding { get; init; }

    /// <summary>
    /// The index of the input that produced this embedding.
    /// </summary>
    public required int Index { get; init; }
}

/// <summary>
/// Token usage for an embedding request.
/// </summary>
public sealed record EmbeddingUsage
{
    /// <summary>
    /// Total number of tokens consumed.
    /// </summary>
    public required int TotalTokens { get; init; }
}
