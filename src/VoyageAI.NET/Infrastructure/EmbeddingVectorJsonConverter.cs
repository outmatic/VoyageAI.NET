using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VoyageAI;

internal sealed class EmbeddingVectorJsonConverter : JsonConverter<float[]>
{
    private readonly string _outputDtype;

    public EmbeddingVectorJsonConverter() : this("float") { }

    internal EmbeddingVectorJsonConverter(string outputDtype) => _outputDtype = outputDtype;

    public override float[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var values = new List<float>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return values.ToArray();
                if (reader.TokenType != JsonTokenType.Number || !reader.TryGetSingle(out var value))
                    throw new JsonException("Embedding arrays must contain only numbers.");
                values.Add(value);
            }
            throw new JsonException("Incomplete embedding array.");
        }

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("An embedding must be a numeric array or a base64 string.");

        var bytes = reader.GetBytesFromBase64();
        if (_outputDtype == "float")
        {
            if (bytes.Length % sizeof(float) != 0)
                throw new JsonException("A base64 float embedding must contain a multiple of four bytes.");

            var values = new float[bytes.Length / sizeof(float)];
            for (var i = 0; i < values.Length; i++)
                values[i] = BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(i * sizeof(float), sizeof(float)));
            return values;
        }

        var integers = new float[bytes.Length];
        var signed = _outputDtype is "int8" or "binary";
        for (var i = 0; i < integers.Length; i++)
            integers[i] = signed ? unchecked((sbyte)bytes[i]) : bytes[i];
        return integers;
    }

    public override void Write(Utf8JsonWriter writer, float[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
            writer.WriteNumberValue(item);
        writer.WriteEndArray();
    }
}
