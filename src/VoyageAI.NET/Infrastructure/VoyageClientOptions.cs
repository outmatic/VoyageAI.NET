namespace VoyageAI;

/// <summary>
/// Configuration options for the Voyage.ai API client.
/// </summary>
public sealed class VoyageClientOptions
{
    /// <summary>
    /// The API key used for authentication. Sent as a Bearer token.
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// The base URL for the Voyage.ai API. Defaults to <c>https://api.voyageai.com/v1/</c>.
    /// </summary>
    public Uri BaseUrl { get; set; } = new("https://api.voyageai.com/v1/");
}
