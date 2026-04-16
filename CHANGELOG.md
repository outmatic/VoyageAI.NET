# Changelog

## 1.0.0

Initial release.

- Text embeddings (`EmbedAsync`)
- Multimodal embeddings (`EmbedMultimodalAsync`)
- Reranking (`RerankAsync`)
- Auto-batching with `IAsyncEnumerable` (`EmbedBatchAsync`, `EmbedMultimodalBatchAsync`)
- Simplified overloads for common use cases
- Built-in resilience via `Microsoft.Extensions.Http.Resilience`
- HTTP/2 by default
- Source-generated JSON serialization
- Multi-target: net8.0, net9.0, net10.0
