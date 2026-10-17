# Changelog

## Unreleased

- New model constants: `VoyageModels.VoyageCode4`, `VoyageModels.Rerank3`, and `VoyageModels.Rerank3Lite`; earlier constants remain available
- Multimodal video content via `VideoUrlContentPart` and `VideoBase64ContentPart`, video pixel usage, output dimensions, and output data types
- Update SourceLink to 10.0.111 to fix the vulnerable transitive `Microsoft.Build.Tasks.Git` dependency (CVE-2026-62900)

## 1.1.0

- Contextualized chunk embeddings (`EmbedContextualizedAsync`, `EmbedContextualizedBatchAsync`) via `/contextualizedembeddings`
- Server-side auto-chunking support (`EnableAutoChunking`, `ChunkSize`, `ChunkOverlap`)
- New model constants: `VoyageModels.VoyageContext4`, `VoyageModels.VoyageContext3`

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
