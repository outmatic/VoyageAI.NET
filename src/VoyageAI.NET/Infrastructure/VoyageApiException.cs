using System.Net;

namespace VoyageAI;

/// <summary>
/// Thrown when the Voyage.ai API returns an error response.
/// </summary>
public sealed class VoyageApiException : Exception
{
    /// <summary>
    /// The HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// The error detail message from the API, if available.
    /// </summary>
    public string? Detail { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="VoyageApiException"/>.
    /// </summary>
    public VoyageApiException(HttpStatusCode statusCode, string? detail)
        : base($"Voyage API error {(int)statusCode}: {detail}")
    {
        StatusCode = statusCode;
        Detail = detail;
    }
}
