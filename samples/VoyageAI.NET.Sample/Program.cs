using System.Net.Http.Headers;
using VoyageAI;
using VoyageAI.Sample.Demos;

var apiKey = Environment.GetEnvironmentVariable("VOYAGE_API_KEY")
             ?? throw new InvalidOperationException("Set the VOYAGE_API_KEY environment variable.");

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.voyageai.com/v1/"),
    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", apiKey) }
};

var client = new VoyageClient(httpClient);

var demos = new (string Name, Func<IVoyageClient, Task> Run)[]
{
    ("Text Embeddings", TextEmbeddingsDemo.RunAsync),
    ("Batch Embeddings", BatchEmbeddingsDemo.RunAsync),
    ("Contextualized Embeddings", ContextualizedEmbeddingsDemo.RunAsync),
    ("Reranking", RerankingDemo.RunAsync),
};

foreach (var (name, run) in demos)
{
    Console.WriteLine($"=== {name} ===");
    await run(client);
    Console.WriteLine();
}
