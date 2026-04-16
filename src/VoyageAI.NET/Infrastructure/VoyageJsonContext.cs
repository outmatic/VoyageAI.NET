using System.Text.Json.Serialization;
using VoyageAI.Models;

namespace VoyageAI;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(EmbeddingRequest))]
[JsonSerializable(typeof(EmbeddingResponse))]
[JsonSerializable(typeof(MultimodalEmbeddingRequest))]
[JsonSerializable(typeof(MultimodalEmbeddingResponse))]
[JsonSerializable(typeof(RerankRequest))]
[JsonSerializable(typeof(RerankResponse))]
[JsonSerializable(typeof(VoyageApiError))]
internal sealed partial class VoyageJsonContext : JsonSerializerContext;
