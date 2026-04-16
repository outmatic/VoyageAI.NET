using System.Runtime.CompilerServices;
using VoyageAI.Models;

namespace VoyageAI;

public sealed partial class VoyageClient
{
    /// <inheritdoc />
    public Task<MultimodalEmbeddingResponse> EmbedMultimodalAsync(MultimodalEmbeddingRequest request, CancellationToken cancellationToken = default)
        => PostAsync("multimodalembeddings", request,
            VoyageJsonContext.Default.MultimodalEmbeddingRequest,
            VoyageJsonContext.Default.MultimodalEmbeddingResponse,
            cancellationToken);

    /// <inheritdoc />
    public async IAsyncEnumerable<MultimodalEmbeddingResponse> EmbedMultimodalBatchAsync(
        IEnumerable<MultimodalInput> inputs,
        string model,
        string? inputType = null,
        int batchSize = 128,
        int maxConcurrency = 8,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var window in inputs.Chunk(batchSize).Chunk(maxConcurrency))
        {
            var tasks = window
                .Select(batch => EmbedMultimodalAsync(
                    new MultimodalEmbeddingRequest { Inputs = batch.ToList(), Model = model, InputType = inputType },
                    cancellationToken))
                .ToList();

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                yield return result;
            }
        }
    }
}
