using VoyageAI.Models;

namespace VoyageAI;

public sealed partial class VoyageClient
{
    /// <inheritdoc />
    public Task<MultimodalEmbeddingResponse> EmbedMultimodalAsync(MultimodalEmbeddingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return PostAsync("multimodalembeddings", request,
            VoyageJsonContext.Default.MultimodalEmbeddingRequest,
            VoyageJsonContext.ForEmbeddingDtype(request.OutputDtype).MultimodalEmbeddingResponse,
            cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<MultimodalEmbeddingResponse> EmbedMultimodalBatchAsync(
        IEnumerable<MultimodalInput> inputs,
        string model,
        string? inputType = null,
        int batchSize = 128,
        int maxConcurrency = 8,
        CancellationToken cancellationToken = default)
        => BatchAsync(inputs, batchSize, maxConcurrency,
            (batch, ct) => EmbedMultimodalAsync(
                new MultimodalEmbeddingRequest { Inputs = batch.ToList(), Model = model, InputType = inputType },
                ct),
            cancellationToken);
}
