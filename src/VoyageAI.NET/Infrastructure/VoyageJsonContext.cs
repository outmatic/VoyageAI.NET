using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using VoyageAI.Models;

namespace VoyageAI;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    Converters = [typeof(EmbeddingVectorJsonConverter)])]
[JsonSerializable(typeof(EmbeddingRequest))]
[JsonSerializable(typeof(EmbeddingResponse))]
[JsonSerializable(typeof(ContextualizedEmbeddingRequest))]
[JsonSerializable(typeof(ContextualizedEmbeddingResponse))]
[JsonSerializable(typeof(MultimodalEmbeddingRequest))]
[JsonSerializable(typeof(MultimodalEmbeddingResponse))]
[JsonSerializable(typeof(RerankRequest))]
[JsonSerializable(typeof(RerankResponse))]
[JsonSerializable(typeof(VoyageApiError))]
[JsonSerializable(typeof(JsonObject))]
internal sealed partial class VoyageJsonContext : JsonSerializerContext
{
    internal static VoyageJsonContext ForEmbeddingDtype(string? outputDtype) => outputDtype switch
    {
        "int8" or "binary" => QuantizedContexts.Signed,
        "uint8" or "ubinary" => QuantizedContexts.Unsigned,
        _ => Default
    };

    private static class QuantizedContexts
    {
        internal static readonly VoyageJsonContext Signed = CreateEmbeddingContext("int8");
        internal static readonly VoyageJsonContext Unsigned = CreateEmbeddingContext("uint8");
    }

    private static VoyageJsonContext CreateEmbeddingContext(string outputDtype)
    {
        var options = new JsonSerializerOptions(Default.Options);
        options.Converters.Insert(0, new EmbeddingVectorJsonConverter(outputDtype));
        return new VoyageJsonContext(options);
    }
}
