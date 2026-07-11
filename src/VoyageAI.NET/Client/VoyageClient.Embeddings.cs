using VoyageAI.Models;

namespace VoyageAI;

public sealed partial class VoyageClient
{
    /// <inheritdoc />
    public Task<EmbeddingResponse> EmbedAsync(EmbeddingRequest request, CancellationToken cancellationToken = default)
        => PostAsync("embeddings", request,
            VoyageJsonContext.Default.EmbeddingRequest,
            VoyageJsonContext.Default.EmbeddingResponse,
            cancellationToken);

    /// <inheritdoc />
    public Task<EmbeddingResponse> EmbedAsync(string input, string model, string? inputType = null, CancellationToken cancellationToken = default)
        => EmbedAsync(new EmbeddingRequest { Input = input, Model = model, InputType = inputType }, cancellationToken);

    /// <inheritdoc />
    public Task<EmbeddingResponse> EmbedAsync(IEnumerable<string> input, string model, string? inputType = null, CancellationToken cancellationToken = default)
        => EmbedAsync(new EmbeddingRequest { Input = input.ToList(), Model = model, InputType = inputType }, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<EmbeddingResponse> EmbedBatchAsync(
        IEnumerable<string> inputs,
        string model,
        string? inputType = null,
        int batchSize = 128,
        int maxConcurrency = 8,
        CancellationToken cancellationToken = default)
        => BatchAsync(inputs, batchSize, maxConcurrency,
            (batch, ct) => EmbedAsync(batch, model, inputType, ct),
            cancellationToken);
}
