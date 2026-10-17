using System.Text.Json.Serialization;

namespace VoyageAI.Models;

/// <summary>
/// Request body for the rerank endpoint.
/// </summary>
public sealed record RerankRequest
{
    /// <summary>
    /// The query to rank documents against.
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// The documents to rerank.
    /// </summary>
    public required List<string> Documents { get; init; }

    /// <summary>
    /// The reranking model to use (e.g. <see cref="VoyageModels.Rerank3"/>).
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Return only the top K results. Returns all results if <c>null</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TopK { get; init; }

    /// <summary>
    /// Whether to include the document text in the response.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ReturnDocuments { get; init; }

    /// <summary>
    /// Whether to truncate inputs that exceed the model's context length. Defaults to <c>true</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Truncation { get; init; }
}

/// <summary>
/// Response from the rerank endpoint.
/// </summary>
public sealed record RerankResponse
{
    /// <summary>
    /// Object type — always <c>"list"</c>.
    /// </summary>
    public required string Object { get; init; }

    /// <summary>
    /// The reranked results, sorted by relevance score descending.
    /// </summary>
    public required List<RerankResult> Data { get; init; }

    /// <summary>
    /// The model used for reranking.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Token usage information.
    /// </summary>
    public required EmbeddingUsage Usage { get; init; }
}

/// <summary>
/// A single reranking result.
/// </summary>
public sealed record RerankResult
{
    /// <summary>
    /// The index of the document in the original input list.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// The relevance score of the document to the query.
    /// </summary>
    public required double RelevanceScore { get; init; }

    /// <summary>
    /// The document text, if <see cref="RerankRequest.ReturnDocuments"/> was <c>true</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Document { get; init; }
}
