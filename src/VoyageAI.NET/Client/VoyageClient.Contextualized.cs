using System.Text.Json;
using VoyageAI.Models;

namespace VoyageAI;

public sealed partial class VoyageClient
{
    /// <inheritdoc />
    public Task<ContextualizedEmbeddingResponse> EmbedContextualizedAsync(ContextualizedEmbeddingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var responseInfo = VoyageJsonContext.ForEmbeddingDtype(request.OutputDtype).ContextualizedEmbeddingResponse;

        if (request.EnableAutoChunking != true)
            return PostAsync("contextualizedembeddings", request,
                VoyageJsonContext.Default.ContextualizedEmbeddingRequest, responseInfo, cancellationToken);

        if (request.InputType != InputType.Document)
            throw new ArgumentException("Auto-chunking requires input_type to be 'document'.", nameof(request));

        var documents = new List<string>(request.Inputs.Count);
        foreach (var chunks in request.Inputs)
        {
            if (chunks is not { Count: 1 })
                throw new ArgumentException("Auto-chunking requires exactly one full-document string per input.", nameof(request));
            documents.Add(chunks[0]);
        }

        // Keep the public nested input API, but send the flat shape required for auto-chunking.
        var payload = JsonSerializer.SerializeToNode(request, VoyageJsonContext.Default.ContextualizedEmbeddingRequest)!.AsObject();
        payload["inputs"] = JsonSerializer.SerializeToNode(documents, VoyageJsonContext.Default.ListString);
        return PostAsync("contextualizedembeddings", payload,
            VoyageJsonContext.Default.JsonObject, responseInfo,
            cancellationToken);
    }

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
