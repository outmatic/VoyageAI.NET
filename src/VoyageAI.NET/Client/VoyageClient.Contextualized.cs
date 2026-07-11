using VoyageAI.Models;

namespace VoyageAI;

public sealed partial class VoyageClient
{
    /// <inheritdoc />
    public Task<ContextualizedEmbeddingResponse> EmbedContextualizedAsync(ContextualizedEmbeddingRequest request, CancellationToken cancellationToken = default)
        => PostAsync("contextualizedembeddings", request,
            VoyageJsonContext.Default.ContextualizedEmbeddingRequest,
            VoyageJsonContext.Default.ContextualizedEmbeddingResponse,
            cancellationToken);

    /// <inheritdoc />
    public Task<ContextualizedEmbeddingResponse> EmbedContextualizedAsync(IEnumerable<IEnumerable<string>> inputs, string model, string? inputType = null, CancellationToken cancellationToken = default)
        => EmbedContextualizedAsync(new ContextualizedEmbeddingRequest
        {
            Inputs = inputs.Select(chunks => chunks.ToList()).ToList(),
            Model = model,
            InputType = inputType
        }, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<ContextualizedEmbeddingResponse> EmbedContextualizedBatchAsync(
        IEnumerable<IEnumerable<string>> inputs,
        string model,
        string? inputType = null,
        int batchSize = 128,
        int maxConcurrency = 8,
        CancellationToken cancellationToken = default)
        => BatchAsync(inputs, batchSize, maxConcurrency,
            (batch, ct) => EmbedContextualizedAsync(batch, model, inputType, ct),
            cancellationToken);
}
