namespace VoyageAI.Sample.Demos;

public static class RerankingDemo
{
    public static async Task RunAsync(IVoyageClient client)
    {
        var documents = new[]
        {
            "The Mediterranean diet emphasizes fish, olive oil, and vegetables, believed to reduce chronic diseases.",
            "Photosynthesis in plants converts light energy into glucose and produces essential oxygen.",
            "20th-century innovations, from radios to smartphones, centered on electronic advancements.",
            "Rivers provide water, irrigation, and habitat for aquatic species, vital for ecosystems.",
            "Apple's conference call to discuss fourth fiscal quarter results and business updates is scheduled for Thursday, November 2, 2023 at 2:00 p.m. PT / 5:00 p.m. ET.",
            "Shakespeare's works, like 'Hamlet' and 'A Midsummer Night's Dream,' endure in literature."
        };

        var response = await client.RerankAsync(
            "When is Apple's conference call scheduled?",
            documents,
            VoyageModels.Rerank3,
            topK: 3);

        foreach (var result in response.Data)
        {
            Console.WriteLine($"  [{result.Index}] score={result.RelevanceScore:F4} → {documents[result.Index]}");
        }

        Console.WriteLine($"Tokens used: {response.Usage.TotalTokens}");
    }
}
