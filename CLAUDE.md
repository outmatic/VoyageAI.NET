# VoyageAI.NET

.NET client library for the [Voyage.ai](https://voyageai.com) API — embeddings, contextualized chunk embeddings, multimodal embeddings, and reranking.

## Project Structure

```
VoyageAI.NET.sln
├── src/VoyageAI.NET/                    → Class library (net8.0;net9.0;net10.0)
│   ├── Client/                          → VoyageClient partial class files
│   │   ├── VoyageClient.cs              → Core (constructor, PostAsync)
│   │   ├── VoyageClient.Batching.cs     → Shared ordered batching and cancellation
│   │   ├── VoyageClient.Embeddings.cs   → EmbedAsync, EmbedBatchAsync
│   │   ├── VoyageClient.Contextualized.cs → EmbedContextualizedAsync, EmbedContextualizedBatchAsync
│   │   ├── VoyageClient.Multimodal.cs   → EmbedMultimodalAsync, EmbedMultimodalBatchAsync
│   │   └── VoyageClient.Rerank.cs       → RerankAsync
│   ├── Models/                          → Request/response record DTOs
│   ├── Extensions/                      → ServiceCollectionExtensions (DI)
│   ├── Constants/                       → VoyageModels, InputType
│   ├── Infrastructure/                  → VoyageJsonContext, VoyageApiException, VoyageClientOptions
│   └── IVoyageClient.cs                 → Public interface
├── tests/VoyageAI.NET.Tests/            → xUnit v3 tests
└── samples/VoyageAI.NET.Sample/         → Console sample app
    └── Demos/                           → One file per demo scenario
```

## Architecture

- **Type**: NuGet class library
- **Pattern**: Typed `HttpClient` via `IHttpClientFactory` with HTTP/2
- **Serialization**: `System.Text.Json` with source generation (`VoyageJsonContext`)
- **Resilience**: `Microsoft.Extensions.Http.Resilience` (retry, circuit breaker, timeout)
- **DI**: `AddVoyageClient(options => ...)` extension method on `IServiceCollection`
- **Models**: Immutable records with `required init` properties

## Voyage.ai API Surface

Base URL: `https://api.voyageai.com/v1`
Auth: Bearer token via `Authorization` header

| Endpoint                  | Method                      | Description                     |
|---------------------------|-----------------------------|---------------------------------|
| `/embeddings`             | `EmbedAsync`                | Text embeddings                 |
| `/embeddings`             | `EmbedBatchAsync`           | Batched text embeddings         |
| `/contextualizedembeddings` | `EmbedContextualizedAsync` | Contextualized chunk embeddings |
| `/contextualizedembeddings` | `EmbedContextualizedBatchAsync` | Batched contextualized embeddings |
| `/multimodalembeddings`   | `EmbedMultimodalAsync`      | Text + image embeddings         |
| `/multimodalembeddings`   | `EmbedMultimodalBatchAsync` | Batched multimodal embeddings   |
| `/rerank`                 | `RerankAsync`               | Document reranking              |

OpenAPI spec: https://github.com/voyage-ai/openapi/blob/main/voyage-openapi.yml

## Build & Test

```bash
dotnet build
dotnet test
```

## Conventions

- **Nullable**: enabled globally
- **Warnings as errors**: enabled
- **Central package management**: `Directory.Packages.props`
- **JSON**: Always use `VoyageJsonContext` (source-gen) — no reflection-based serialization
- **Models**: Immutable records in `Models/` folder with `required init` properties
- **Embeddings**: `float[]` (not `double`) — standard for vector databases
- **Async**: All public API methods are async with `CancellationToken` support
- **Batching**: `IAsyncEnumerable` with `Chunk()` and a bounded queue of tasks; preserve order and cancel outstanding requests when enumeration stops
- **Errors**: API errors throw `VoyageApiException` with status code and detail
- **Testing**: xUnit v3 — use `TestContext.Current.CancellationToken` in async tests
- **Constants**: Use `VoyageModels.*` and `InputType.*` instead of magic strings
