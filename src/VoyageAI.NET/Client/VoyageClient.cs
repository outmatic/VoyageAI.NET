using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using VoyageAI.Models;

namespace VoyageAI;

/// <summary>
/// HTTP client for the Voyage.ai API.
/// </summary>
public sealed partial class VoyageClient : IVoyageClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="VoyageClient"/> with the specified <see cref="HttpClient"/>.
    /// </summary>
    public VoyageClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        JsonTypeInfo<TRequest> requestInfo,
        JsonTypeInfo<TResponse> responseInfo,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(endpoint, request, requestInfo, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            VoyageApiError? error = null;
            try
            {
                error = await response.Content.ReadFromJsonAsync(VoyageJsonContext.Default.VoyageApiError, cancellationToken);
            }
            catch (JsonException)
            {
                // Non-JSON error body (e.g. HTML 502 from an upstream CDN) — surface status only.
            }
            throw new VoyageApiException(response.StatusCode, error?.Detail);
        }

        return (await response.Content.ReadFromJsonAsync(responseInfo, cancellationToken))!;
    }
}
