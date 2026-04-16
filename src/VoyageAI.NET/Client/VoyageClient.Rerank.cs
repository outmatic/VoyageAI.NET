using VoyageAI.Models;

namespace VoyageAI;

public sealed partial class VoyageClient
{
    /// <inheritdoc />
    public Task<RerankResponse> RerankAsync(RerankRequest request, CancellationToken cancellationToken = default)
        => PostAsync("rerank", request,
            VoyageJsonContext.Default.RerankRequest,
            VoyageJsonContext.Default.RerankResponse,
            cancellationToken);

    /// <inheritdoc />
    public Task<RerankResponse> RerankAsync(string query, IEnumerable<string> documents, string model, int? topK = null, CancellationToken cancellationToken = default)
        => RerankAsync(new RerankRequest { Query = query, Documents = documents.ToList(), Model = model, TopK = topK }, cancellationToken);
}
