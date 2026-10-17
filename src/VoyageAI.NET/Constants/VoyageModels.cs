namespace VoyageAI;

/// <summary>
/// Known Voyage.ai model identifiers.
/// </summary>
public static class VoyageModels
{
    // ── Embedding models ──

    /// <summary>voyage-4-large</summary>
    public const string Voyage4Large = "voyage-4-large";

    /// <summary>voyage-4</summary>
    public const string Voyage4 = "voyage-4";

    /// <summary>voyage-4-lite</summary>
    public const string Voyage4Lite = "voyage-4-lite";

    /// <summary>voyage-code-4 — optimized for code retrieval and coding agents.</summary>
    public const string VoyageCode4 = "voyage-code-4";

    /// <summary>voyage-code-3</summary>
    public const string VoyageCode3 = "voyage-code-3";

    /// <summary>voyage-finance-2</summary>
    public const string VoyageFinance2 = "voyage-finance-2";

    /// <summary>voyage-law-2</summary>
    public const string VoyageLaw2 = "voyage-law-2";

    /// <summary>voyage-code-2</summary>
    public const string VoyageCode2 = "voyage-code-2";

    // ── Contextualized embedding models ──

    /// <summary>voyage-context-4</summary>
    public const string VoyageContext4 = "voyage-context-4";

    /// <summary>voyage-context-3</summary>
    public const string VoyageContext3 = "voyage-context-3";

    // ── Multimodal embedding models ──

    /// <summary>voyage-multimodal-3.5</summary>
    public const string VoyageMultimodal35 = "voyage-multimodal-3.5";

    // ── Rerank models ──

    /// <summary>rerank-3 — highest retrieval accuracy.</summary>
    public const string Rerank3 = "rerank-3";

    /// <summary>rerank-3-lite — optimized for latency and cost.</summary>
    public const string Rerank3Lite = "rerank-3-lite";

    /// <summary>rerank-2.5</summary>
    public const string Rerank25 = "rerank-2.5";

    /// <summary>rerank-2.5-lite</summary>
    public const string Rerank25Lite = "rerank-2.5-lite";
}
