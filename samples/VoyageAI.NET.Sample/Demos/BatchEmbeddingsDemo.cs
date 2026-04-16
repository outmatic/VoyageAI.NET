namespace VoyageAI.Sample.Demos;

public static class BatchEmbeddingsDemo
{
    public static async Task RunAsync(IVoyageClient client)
    {
        var texts = Enumerable.Range(1, 10)
            .Select(i => $"Document number {i} about various topics.")
            .ToList();

        var totalEmbeddings = 0;

        await foreach (var batch in client.EmbedBatchAsync(texts, VoyageModels.Voyage4, InputType.Document, batchSize: 5))
        {
            totalEmbeddings += batch.Data.Count;
            Console.WriteLine($"Batch received: {batch.Data.Count} embeddings, {batch.Usage.TotalTokens} tokens");
        }

        Console.WriteLine($"Total embeddings: {totalEmbeddings}");
    }
}
