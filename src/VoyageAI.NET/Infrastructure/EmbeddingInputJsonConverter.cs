using System.Text.Json;
using System.Text.Json.Serialization;

namespace VoyageAI;

internal sealed class EmbeddingInputJsonConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString()!,
            JsonTokenType.StartArray => JsonSerializer.Deserialize(ref reader, VoyageJsonContext.Default.ListString)!,
            _ => throw new JsonException("Embedding input must be a string or a sequence of strings.")
        };

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case string text:
                writer.WriteStringValue(text);
                break;
            case IEnumerable<string> texts:
                writer.WriteStartArray();
                foreach (var text in texts)
                    writer.WriteStringValue(text);
                writer.WriteEndArray();
                break;
            default:
                throw new JsonException("Embedding input must be a string or a sequence of strings.");
        }
    }
}
