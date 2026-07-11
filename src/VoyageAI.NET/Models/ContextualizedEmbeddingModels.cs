using System.Text.Json.Serialization;

namespace VoyageAI.Models;

/// <summary>
/// Request body for the contextualized chunk embeddings endpoint.
/// Each inner list of <see cref="Inputs"/> holds the chunks of one document;
/// chunks are embedded with awareness of their sibling chunks.
/// </summary>
public sealed record ContextualizedEmbeddingRequest
{
    /// <summary>
    /// The inputs to embed — one list of chunks per document (up to 1,000 inputs,
    /// 120K total tokens, 16K total chunks).
    /// </summary>
    public required List<List<string>> Inputs { get; init; }

    /// <summary>
    /// The model to use (e.g. <see cref="VoyageModels.VoyageContext4"/>).
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Optional input type hint: <see cref="InputType.Query"/> or <see cref="InputType.Document"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InputType { get; init; }

    /// <summary>
    /// Desired output embedding dimension: 256, 512, 1024 (default), or 2048.
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

    /// <summary>
    /// Whether the server should chunk each document automatically. Requires
    /// <see cref="InputType"/> = <see cref="VoyageAI.InputType.Document"/> and exactly one
    /// string per document in <see cref="Inputs"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? EnableAutoChunking { get; init; }

    /// <summary>
    /// Target chunk size in tokens for auto-chunking (default 512, max 32K).
    /// Requires <see cref="EnableAutoChunking"/> = <c>true</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ChunkSize { get; init; }

    /// <summary>
    /// Token overlap between consecutive auto-chunks (default 0, must be smaller than
    /// <see cref="ChunkSize"/>). Requires <see cref="EnableAutoChunking"/> = <c>true</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ChunkOverlap { get; init; }
}

/// <summary>
/// Response from the contextualized chunk embeddings endpoint.
/// </summary>
public sealed record ContextualizedEmbeddingResponse
{
    /// <summary>
    /// One result per input document, in input order.
    /// </summary>
    public required List<ContextualizedEmbeddingDocument> Data { get; init; }

    /// <summary>
    /// The model used to generate the embeddings.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Token usage information.
    /// </summary>
    public required EmbeddingUsage Usage { get; init; }

    /// <summary>
    /// Version of the server-side chunker, when auto-chunking was used.
    /// </summary>
    public string? ChunkerVersion { get; init; }
}

/// <summary>
/// Contextualized embeddings for the chunks of a single input document.
/// </summary>
public sealed record ContextualizedEmbeddingDocument
{
    /// <summary>
    /// One embedding per chunk, in chunk order.
    /// </summary>
    public required List<ContextualizedChunkEmbedding> Data { get; init; }

    /// <summary>
    /// The index of the input document that produced these embeddings.
    /// </summary>
    public required int Index { get; init; }
}

/// <summary>
/// A single contextualized chunk embedding.
/// </summary>
public sealed record ContextualizedChunkEmbedding
{
    /// <summary>
    /// The embedding vector.
    /// </summary>
    public required float[] Embedding { get; init; }

    /// <summary>
    /// The index of the chunk within its document.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// The chunk text, when the server performed the chunking (auto-chunking).
    /// </summary>
    public string? Text { get; init; }
}
