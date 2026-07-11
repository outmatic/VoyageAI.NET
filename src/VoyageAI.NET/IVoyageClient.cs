using VoyageAI.Models;

namespace VoyageAI;

/// <summary>
/// Client interface for the Voyage.ai API.
/// </summary>
public interface IVoyageClient
{
    /// <summary>
    /// Creates embeddings for the given request.
    /// </summary>
    Task<EmbeddingResponse> EmbedAsync(EmbeddingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an embedding for a single text input.
    /// </summary>
    Task<EmbeddingResponse> EmbedAsync(string input, string model, string? inputType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates embeddings for multiple text inputs.
    /// </summary>
    Task<EmbeddingResponse> EmbedAsync(IEnumerable<string> input, string model, string? inputType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates embeddings for a large number of inputs, automatically batching and parallelizing requests.
    /// Results are streamed as each batch completes, preserving input order.
    /// </summary>
    IAsyncEnumerable<EmbeddingResponse> EmbedBatchAsync(IEnumerable<string> inputs, string model, string? inputType = null, int batchSize = 128, int maxConcurrency = 8, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates contextualized chunk embeddings, where each chunk is embedded with
    /// awareness of the other chunks in the same document.
    /// </summary>
    Task<ContextualizedEmbeddingResponse> EmbedContextualizedAsync(ContextualizedEmbeddingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates contextualized chunk embeddings for the given documents, where each inner
    /// sequence holds the chunks of one document.
    /// </summary>
    Task<ContextualizedEmbeddingResponse> EmbedContextualizedAsync(IEnumerable<IEnumerable<string>> inputs, string model, string? inputType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates contextualized chunk embeddings for a large number of documents, automatically
    /// batching and parallelizing requests. Results are streamed as each batch completes,
    /// preserving input order. Batching is per document; chunks of a document always stay
    /// in the same request.
    /// </summary>
    IAsyncEnumerable<ContextualizedEmbeddingResponse> EmbedContextualizedBatchAsync(IEnumerable<IEnumerable<string>> inputs, string model, string? inputType = null, int batchSize = 128, int maxConcurrency = 8, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates embeddings for multimodal input(s) containing text and/or images.
    /// </summary>
    Task<MultimodalEmbeddingResponse> EmbedMultimodalAsync(MultimodalEmbeddingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multimodal embeddings for a large number of inputs, automatically batching and parallelizing requests.
    /// Results are streamed as each batch completes, preserving input order.
    /// </summary>
    IAsyncEnumerable<MultimodalEmbeddingResponse> EmbedMultimodalBatchAsync(IEnumerable<MultimodalInput> inputs, string model, string? inputType = null, int batchSize = 128, int maxConcurrency = 8, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reranks documents by relevance to a query.
    /// </summary>
    Task<RerankResponse> RerankAsync(RerankRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reranks documents by relevance to a query.
    /// </summary>
    Task<RerankResponse> RerankAsync(string query, IEnumerable<string> documents, string model, int? topK = null, CancellationToken cancellationToken = default);
}
