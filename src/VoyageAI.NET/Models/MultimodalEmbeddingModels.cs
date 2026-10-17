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
    /// Desired output embedding dimension for models that support it, such as voyage-multimodal-3.5.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? OutputDimension { get; init; }

    /// <summary>
    /// Output data type for models that support it: <c>"float"</c>, <c>"int8"</c>,
    /// <c>"uint8"</c>, <c>"binary"</c>, or <c>"ubinary"</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OutputDtype { get; init; }

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
    /// The content parts (text, images, or videos). Video requires voyage-multimodal-3.5.
    /// </summary>
    public required List<ContentPart> Content { get; init; }
}

/// <summary>
/// Base type for multimodal content parts.
/// </summary>
[JsonDerivedType(typeof(TextContentPart))]
[JsonDerivedType(typeof(ImageUrlContentPart))]
[JsonDerivedType(typeof(ImageBase64ContentPart))]
[JsonDerivedType(typeof(VideoUrlContentPart))]
[JsonDerivedType(typeof(VideoBase64ContentPart))]
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
    /// The base64-encoded image in data URL format, e.g. <c>data:image/png;base64,...</c>.
    /// </summary>
    public required string ImageBase64 { get; init; }
}

/// <summary>
/// An MP4 video referenced by URL. Supported by voyage-multimodal-3.5.
/// </summary>
public sealed record VideoUrlContentPart : ContentPart
{
    /// <inheritdoc />
    public override string Type => "video_url";

    /// <summary>
    /// The video URL.
    /// </summary>
    public required string VideoUrl { get; init; }
}

/// <summary>
/// An MP4 video encoded as base64. Supported by voyage-multimodal-3.5.
/// </summary>
public sealed record VideoBase64ContentPart : ContentPart
{
    /// <inheritdoc />
    public override string Type => "video_base64";

    /// <summary>
    /// The base64-encoded video in data URL format: <c>data:video/mp4;base64,...</c>.
    /// </summary>
    public required string VideoBase64 { get; init; }
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
    /// Number of video pixels processed, or zero when the API omits video usage.
    /// </summary>
    public long VideoPixels { get; init; }

    /// <summary>
    /// Total number of tokens consumed.
    /// </summary>
    public required int TotalTokens { get; init; }
}
