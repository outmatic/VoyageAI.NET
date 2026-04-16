namespace VoyageAI.Sample.Demos;

public static class TextEmbeddingsDemo
{
    public static async Task RunAsync(IVoyageClient client)
    {
        var response = await client.EmbedAsync(
            "The quick brown fox jumps over the lazy dog",
            VoyageModels.Voyage4,
            InputType.Query);

        var embedding = response.Data[0].Embedding;

        Console.WriteLine($"Model: {response.Model}");
        Console.WriteLine($"Dimensions: {embedding.Length}");
        Console.WriteLine($"First 5 values: [{string.Join(", ", embedding.Take(5).Select(v => v.ToString("F6")))}]");
        Console.WriteLine($"Tokens used: {response.Usage.TotalTokens}");
    }
}
