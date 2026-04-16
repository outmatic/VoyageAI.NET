using System.Text.Json.Serialization;

namespace VoyageAI.Models;

/// <summary>
/// Request body for the multimodal embeddings endpoint.
/// </summary>
public sealed record MultimodalEmbeddingRequest
{
    /// <summary>
    /// The multimodal inputs to embed.
    /// </summary>
    public required List<MultimodalInput> Inputs { get; init; }

    /// <summary>
    /// The model to use (e.g. <see cref="VoyageModels.VoyageMultimodal35"/>).
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
    /// Encoding format for the response: <c>null</c> (default JSON arrays) or <c>"base64"</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OutputEncoding { get; init; }
}

/// <summary>
/// A single multimodal input containing mixed content parts.
/// </summary>
public sealed record MultimodalInput
{
    /// <summary>
    /// The content parts (text, image URL, or base64 image).
    /// </summary>
    public required List<ContentPart> Content { get; init; }
}

/// <summary>
/// Base type for multimodal content parts.
/// </summary>
[JsonDerivedType(typeof(TextContentPart))]
[JsonDerivedType(typeof(ImageUrlContentPart))]
[JsonDerivedType(typeof(ImageBase64ContentPart))]
public abstract record ContentPart
{
    /// <summary>
    /// The content type discriminator.
    /// </summary>
    public abstract string Type { get; }
}

/// <summary>
/// A text content part.
/// </summary>
public sealed record TextContentPart : ContentPart
{
    /// <inheritdoc />
    public override string Type => "text";

    /// <summary>
    /// The text content.
    /// </summary>
    public required string Text { get; init; }
}

/// <summary>
/// An image content part referenced by URL.
/// </summary>
public sealed record ImageUrlContentPart : ContentPart
{
    /// <inheritdoc />
    public override string Type => "image_url";

    /// <summary>
    /// The image URL.
    /// </summary>
    public required string ImageUrl { get; init; }
}

/// <summary>
/// An image content part encoded as base64.
/// </summary>
public sealed record ImageBase64ContentPart : ContentPart
{
    /// <inheritdoc />
    public override string Type => "image_base64";

    /// <summary>
    /// The base64-encoded image data.
    /// </summary>
    public required string ImageBase64 { get; init; }
}

/// <summary>
/// Response from the multimodal embeddings endpoint.
/// </summary>
public sealed record MultimodalEmbeddingResponse
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
    /// Token and pixel usage information.
    /// </summary>
    public required MultimodalUsage Usage { get; init; }
}

/// <summary>
/// Token and pixel usage for a multimodal embedding request.
/// </summary>
public sealed record MultimodalUsage
{
    /// <summary>
    /// Number of text tokens consumed.
    /// </summary>
    public required int TextTokens { get; init; }

    /// <summary>
    /// Number of image pixels processed.
    /// </summary>
    public required int ImagePixels { get; init; }

    /// <summary>
    /// Total number of tokens consumed.
    /// </summary>
    public required int TotalTokens { get; init; }
}
