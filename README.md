# VoyageAI.NET

**Typed .NET client for the [Voyage AI](https://voyageai.com) API**

Text embeddings, contextualized chunk embeddings, multimodal embeddings (text, images, and video), and reranking. Source-generated JSON, auto-batching, and built-in resilience.

[![NuGet](https://img.shields.io/nuget/v/VoyageAI.NET?style=flat-square&color=004880)](https://www.nuget.org/packages/VoyageAI.NET)
[![Downloads](https://img.shields.io/nuget/dt/VoyageAI.NET?style=flat-square&color=004880)](https://www.nuget.org/packages/VoyageAI.NET)
[![Build](https://img.shields.io/github/actions/workflow/status/outmatic/VoyageAI.NET/ci.yml?style=flat-square)](https://github.com/outmatic/VoyageAI.NET/actions)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](https://github.com/outmatic/VoyageAI.NET/blob/main/LICENSE)
![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-512bd4?style=flat-square)

---

Wraps the [Voyage AI REST API](https://docs.voyageai.com) embedding and reranking endpoints with typed `HttpClient` via `IHttpClientFactory`, HTTP/2 with ALPN, automatic decompression, pooled connections with DNS refresh, source-generated JSON (AOT- and trim-safe), streaming batch operations via `IAsyncEnumerable`, and automatic retry / circuit breaker / timeout via `Microsoft.Extensions.Http.Resilience`.

```
thousands of texts  ──>  EmbedBatchAsync  ──>  IAsyncEnumerable<EmbeddingResponse>
                         auto-batched                streamed in order
                         8x parallel                 resilient (retry + CB)
```

## Install

```bash
dotnet add package VoyageAI.NET
```

## 30-second Example

```csharp
// Registration
builder.Services.AddVoyageClient(options =>
{
    options.ApiKey = builder.Configuration["Voyage:ApiKey"]!;
});

// Injection
public class SearchService(IVoyageClient voyage)
{
    public async Task<float[]> EmbedAsync(string text, CancellationToken ct)
    {
        var response = await voyage.EmbedAsync(text, VoyageModels.Voyage4, InputType.Query, ct);
        return response.Data[0].Embedding;
    }
}
```

Without DI:

```csharp
using var http = new HttpClient
{
    BaseAddress = new Uri("https://api.voyageai.com/v1/"),
    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", apiKey) }
};

var client = new VoyageClient(http);
```

## Text Embeddings

```csharp
// Single
var r = await client.EmbedAsync("hello world", VoyageModels.Voyage4, InputType.Query);

// Multiple
var r = await client.EmbedAsync(["hello", "world"], VoyageModels.Voyage4);

// Full control
var r = await client.EmbedAsync(new EmbeddingRequest
{
    Input = "hello world",                  // string or IEnumerable<string>
    Model = VoyageModels.Voyage4,           // required
    InputType = InputType.Query,            // "query" or "document" (optional)
    OutputDimension = 512,                  // 256, 512, 1024, 2048 (optional)
    OutputDtype = "float",                  // "float", "int8", "uint8", "binary", "ubinary"
    EncodingFormat = "base64",              // null or "base64" (optional)
    Truncation = true                       // default: true
});

// r.Data[0].Embedding  → float[]
// r.Usage.TotalTokens  → int

// Code retrieval
var code = await client.EmbedAsync(
    "public async Task SaveAsync() { /* ... */ }",
    VoyageModels.VoyageCode4,
    InputType.Document);
```

Base64 responses are decoded into `float[]` automatically. Quantized output types retain their integer or packed-byte values in the returned floats; the client does not dequantize or unpack them.

## Batch Embeddings

Embed thousands of texts with automatic batching and concurrency control. Results stream via `IAsyncEnumerable` in input order. Each consumed result frees a slot for the next request, up to `maxConcurrency` requests at once. Breaking out of enumeration, cancellation, or a request failure cancels outstanding requests.

```csharp
var all = new List<float[]>();

await foreach (var r in client.EmbedBatchAsync(
    thousandsOfTexts,
    VoyageModels.Voyage4,
    inputType: InputType.Document,
    batchSize: 128,        // max per request (default: 128)
    maxConcurrency: 8))    // parallel requests (default: 8)
{
    foreach (var item in r.Data)
        all.Add(item.Embedding);
}
```

## Contextualized Embeddings

Embed document chunks with awareness of their sibling chunks ([contextualized chunk embeddings](https://docs.voyageai.com/docs/contextualized-chunk-embeddings)). Each inner list holds the chunks of one document.

```csharp
// Simple — one list of chunks per document
var r = await client.EmbedContextualizedAsync(
    [
        ["doc1 chunk1", "doc1 chunk2"],
        ["doc2 chunk1"]
    ],
    VoyageModels.VoyageContext4,
    InputType.Document);

// Full control — including server-side auto-chunking
var r = await client.EmbedContextualizedAsync(new ContextualizedEmbeddingRequest
{
    Inputs = [["full document text"]],      // one full document per inner list
    Model = VoyageModels.VoyageContext4,    // required
    InputType = InputType.Document,         // required for auto-chunking
    EnableAutoChunking = true,              // let the server chunk each document
    ChunkSize = 512,                        // tokens per chunk (default: 512)
    ChunkOverlap = 0,                       // token overlap between chunks (default: 0)
    OutputDimension = 1024                  // 256, 512, 1024, 2048 (optional)
});

// r.Data[0].Index               → int (document position)
// r.Data[0].Data[0].Embedding   → float[] (per-chunk vector)
// r.Data[0].Data[0].Text        → string? (chunk text, when auto-chunked)
// r.Usage.TotalTokens           → int

// Batch — batching is per document; a document's chunks stay in one request
await foreach (var r in client.EmbedContextualizedBatchAsync(
    documents, VoyageModels.VoyageContext4, InputType.Document))
{
    // process
}
```

For auto-chunking, the client sends a flat list of document strings as required by the API. The public request keeps the same nested `Inputs` shape as pre-chunked requests. Each inner list must contain exactly one string, and `InputType` must be `Document`.

## Multimodal Embeddings

```csharp
var r = await client.EmbedMultimodalAsync(new MultimodalEmbeddingRequest
{
    Inputs =
    [
        new MultimodalInput
        {
            Content =
            [
                new TextContentPart { Text = "A photo of a cat" },
                new ImageUrlContentPart { ImageUrl = "https://example.com/cat.jpg" },
                new VideoUrlContentPart { VideoUrl = "https://example.com/cat.mp4" }
                // Base64 alternatives: ImageBase64ContentPart / VideoBase64ContentPart
                // with data URLs such as "data:video/mp4;base64,..."
            ]
        }
    ],
    Model = VoyageModels.VoyageMultimodal35,
    InputType = InputType.Document,
    OutputDimension = 512,
    OutputDtype = "float"
});

// r.Data[0].Embedding    → float[]
// r.Usage.TextTokens     → int
// r.Usage.ImagePixels    → int
// r.Usage.VideoPixels    → long (0 if omitted)
// r.Usage.TotalTokens    → int

// Batch — same pattern as text
await foreach (var r in client.EmbedMultimodalBatchAsync(
    inputs, VoyageModels.VoyageMultimodal35, batchSize: 32, maxConcurrency: 4))
{
    // process
}
```

Video inputs require `voyage-multimodal-3.5`. Use URL content parts throughout a request, or base64 content parts throughout a request; do not mix the two image/video representations. Set `OutputEncoding = "base64"` to receive embeddings decoded automatically into `float[]`.

## Reranking

```csharp
// Simple
var r = await client.RerankAsync(
    "When is Apple's conference call scheduled?",
    documents,
    VoyageModels.Rerank3,
    topK: 3);

// Full control
var r = await client.RerankAsync(new RerankRequest
{
    Query = "When is Apple's conference call scheduled?",
    Documents =
    [
        "The Mediterranean diet emphasizes fish, olive oil, and vegetables...",
        "Apple's conference call is scheduled for Thursday, November 2, 2023...",
        "Shakespeare's works endure in literature..."
    ],
    Model = VoyageModels.Rerank3,
    TopK = 3,
    ReturnDocuments = true,
    Truncation = true
});

// r.Data[0].Index           → int (original position)
// r.Data[0].RelevanceScore  → double
// r.Data[0].Document        → string? (if ReturnDocuments = true)
// r.Usage.TotalTokens       → int
```

## Models

Use `VoyageModels.*` constants — no magic strings. Model identifiers are also accepted as strings, so newly released API models can be used before a package update. The catalogue below follows the official [text embedding](https://docs.voyageai.com/docs/embeddings), [contextualized embedding](https://docs.voyageai.com/docs/contextualized-chunk-embeddings), [multimodal](https://docs.voyageai.com/docs/multimodal-embeddings), and [reranking](https://docs.voyageai.com/docs/reranker) documentation.

### Embedding

| Constant | API id |
| --- | --- |
| `VoyageModels.Voyage4Large` | `voyage-4-large` |
| `VoyageModels.Voyage4` | `voyage-4` |
| `VoyageModels.Voyage4Lite` | `voyage-4-lite` |
| `VoyageModels.VoyageCode4` | `voyage-code-4` |
| `VoyageModels.VoyageCode3` | `voyage-code-3` |
| `VoyageModels.VoyageFinance2` | `voyage-finance-2` |
| `VoyageModels.VoyageLaw2` | `voyage-law-2` |
| `VoyageModels.VoyageCode2` | `voyage-code-2` |

### Contextualized

| Constant | API id |
| --- | --- |
| `VoyageModels.VoyageContext4` | `voyage-context-4` |
| `VoyageModels.VoyageContext3` | `voyage-context-3` |

### Multimodal

| Constant | API id |
| --- | --- |
| `VoyageModels.VoyageMultimodal35` | `voyage-multimodal-3.5` |

### Rerank

| Constant | API id |
| --- | --- |
| `VoyageModels.Rerank3` | `rerank-3` |
| `VoyageModels.Rerank3Lite` | `rerank-3-lite` |
| `VoyageModels.Rerank25` | `rerank-2.5` |
| `VoyageModels.Rerank25Lite` | `rerank-2.5-lite` |

Earlier model constants remain available for compatibility. Open-weight models such as `voyage-4-nano` run locally and are outside this HTTP client's scope.

## Error Handling

API errors throw `VoyageApiException`:

```csharp
try
{
    var r = await client.EmbedAsync(request);
}
catch (VoyageApiException ex)
{
    Console.WriteLine(ex.StatusCode);  // e.g., 400
    Console.WriteLine(ex.Detail);      // e.g., "Invalid model name"
}
```

## HTTP Stack & Resilience

`AddVoyageClient` configures a production-ready HTTP stack out of the box:

- **HTTP/2** via ALPN (`HttpVersionPolicy.RequestVersionOrLower`) with graceful HTTP/1.1 fallback
- **`SocketsHttpHandler`** with `PooledConnectionLifetime = 5min` and `PooledConnectionIdleTimeout = 2min` — refreshes DNS without killing in-flight requests
- **`AutomaticDecompression = All`** (gzip, brotli, deflate)
- **`EnableMultipleHttp2Connections`** to avoid HTTP/2 stream-concurrency limits under load
- **Standard resilience** via [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience): exponential-backoff retry on 429/5xx, circuit breaker, request timeout
- **AOT- and trim-safe**: source-generated `System.Text.Json`, zero reflection, `IsAotCompatible=true`

## Configuration

```csharp
builder.Services.AddVoyageClient(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = new Uri("https://api.voyageai.com/v1/");  // default
});
```

Configuration is validated when the client is resolved: `ApiKey` must be nonblank, and `BaseUrl` must be an absolute HTTP(S) URL without a query or fragment. A missing trailing slash is added automatically.

## Supported Frameworks

`net8.0` · `net9.0` · `net10.0`

## Project Layout

```
src/VoyageAI.NET/
  Client/           VoyageClient partials (core, Batching, Embeddings, Contextualized, Multimodal, Rerank)
  Models/           Request/response records (immutable, `required init`)
  Extensions/       AddVoyageClient DI extension
  Constants/        VoyageModels, InputType
  Infrastructure/   VoyageJsonContext (source-gen), JSON converters, VoyageApiException, options
```

## Samples

Runnable console sample in [`samples/VoyageAI.NET.Sample`](https://github.com/outmatic/VoyageAI.NET/tree/main/samples/VoyageAI.NET.Sample):

```bash
export VOYAGE_API_KEY=your-key
dotnet run --project samples/VoyageAI.NET.Sample
```

## Build & Test

```bash
dotnet build
dotnet test
```

## License

[MIT](https://github.com/outmatic/VoyageAI.NET/blob/main/LICENSE). Built by [Outmatic](https://outmatic.pl).
