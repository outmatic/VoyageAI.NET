namespace VoyageAI;

/// <summary>
/// Input type hints for embedding requests.
/// </summary>
public static class InputType
{
    /// <summary>
    /// Use when the input will be used as a search query.
    /// </summary>
    public const string Query = "query";

    /// <summary>
    /// Use when the input is a document to be indexed.
    /// </summary>
    public const string Document = "document";
}
