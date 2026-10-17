using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace VoyageAI.Extensions;

/// <summary>
/// Extension methods for registering VoyageAI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Voyage.ai client to the service collection with standard resilience (retry, circuit breaker, timeout),
    /// HTTP/2, automatic decompression, and a 5-minute pooled connection lifetime for DNS refresh.
    /// </summary>
    public static IServiceCollection AddVoyageClient(this IServiceCollection services, Action<VoyageClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<VoyageClientOptions>()
            .Configure(configure)
            .Validate(static options => !string.IsNullOrWhiteSpace(options.ApiKey), "A Voyage API key is required.")
            .Validate(static options => options.BaseUrl is { IsAbsoluteUri: true } uri
                && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                && uri.Query.Length == 0 && uri.Fragment.Length == 0,
                "The Voyage base URL must be an absolute HTTP(S) URL without a query or fragment.");

        services.AddHttpClient<IVoyageClient, VoyageClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<VoyageClientOptions>>().Value;
            client.BaseAddress = NormalizeBaseUrl(options.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            AutomaticDecompression = DecompressionMethods.All,
            EnableMultipleHttp2Connections = true
        })
        .AddStandardResilienceHandler();

        return services;
    }

    private static Uri NormalizeBaseUrl(Uri baseUrl)
        => baseUrl.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
            ? baseUrl
            : new Uri($"{baseUrl.AbsoluteUri}/");
}
