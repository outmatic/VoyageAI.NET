using System.Net;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace VoyageAI.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVoyageClient_RegistersIVoyageClient()
    {
        var services = new ServiceCollection();

        services.AddVoyageClient(o =>
        {
            o.ApiKey = "test-key";
        });

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IVoyageClient>();

        Assert.NotNull(client);
        Assert.IsType<VoyageClient>(client);
    }

    [Fact]
    public void AddVoyageClient_UsesV1BaseUrlWithTrailingSlashByDefault()
    {
        var services = new ServiceCollection();
        services.AddVoyageClient(o => o.ApiKey = "test-key");

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IVoyageClient>();

        var httpClient = GetHttpClient(client);
        Assert.Equal(new Uri("https://api.voyageai.com/v1/"), httpClient.BaseAddress);
    }

    [Fact]
    public void AddVoyageClient_NormalizesConfiguredBaseUrlWithoutTrailingSlash()
    {
        var services = new ServiceCollection();
        services.AddVoyageClient(o =>
        {
            o.ApiKey = "test-key";
            o.BaseUrl = new Uri("https://example.com/api/v1");
        });

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IVoyageClient>();

        var httpClient = GetHttpClient(client);
        Assert.Equal(new Uri("https://example.com/api/v1/"), httpClient.BaseAddress);
    }

    [Fact]
    public void AddVoyageClient_SetsBearerAuthorizationHeader()
    {
        var services = new ServiceCollection();
        services.AddVoyageClient(o => o.ApiKey = "my-secret-key");

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IVoyageClient>();
        var httpClient = GetHttpClient(client);

        Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization?.Scheme);
        Assert.Equal("my-secret-key", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
    }

    [Fact]
    public void AddVoyageClient_RequestsHttp2ByDefault()
    {
        var services = new ServiceCollection();
        services.AddVoyageClient(o => o.ApiKey = "test-key");

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IVoyageClient>();
        var httpClient = GetHttpClient(client);

        Assert.Equal(HttpVersion.Version20, httpClient.DefaultRequestVersion);
        Assert.Equal(HttpVersionPolicy.RequestVersionOrHigher, httpClient.DefaultVersionPolicy);
    }

    private static HttpClient GetHttpClient(IVoyageClient client)
    {
        var field = typeof(VoyageClient).GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<HttpClient>(field.GetValue(client));
    }
}
