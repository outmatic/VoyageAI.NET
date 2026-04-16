namespace VoyageAI.Models;

/// <summary>
/// Error response body from the Voyage.ai API.
/// </summary>
public sealed record VoyageApiError
{
    /// <summary>
    /// The error detail message.
    /// </summary>
    public required string Detail { get; init; }
}
