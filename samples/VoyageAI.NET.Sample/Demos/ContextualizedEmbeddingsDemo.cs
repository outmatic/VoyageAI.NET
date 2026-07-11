namespace VoyageAI.Sample.Demos;

public static class ContextualizedEmbeddingsDemo
{
    public static async Task RunAsync(IVoyageClient client)
    {
        // Each inner list holds the chunks of one document; every chunk is embedded
        // with awareness of its sibling chunks.
        var response = await client.EmbedContextualizedAsync(
            inputs:
            [
                [
                    "The Solar System has eight planets.",
                    "Mercury is the closest one to the Sun.",
                    "Neptune is the farthest."
                ],
                [
                    "Voyage AI provides embedding models.",
                    "Contextualized embeddings encode chunks with document context."
                ]
            ],
            VoyageModels.VoyageContext4,
            InputType.Document);

        Console.WriteLine($"Model: {response.Model}");
        foreach (var document in response.Data)
        {
            Console.WriteLine($"Document {document.Index}: {document.Data.Count} chunk embeddings, {document.Data[0].Embedding.Length} dimensions");
        }
        Console.WriteLine($"Tokens used: {response.Usage.TotalTokens}");
    }
}
