using System.Net;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IVoyageClient>();

        Assert.NotNull(client);
        Assert.IsType<VoyageClient>(client);
    }

    [Fact]
    public void AddVoyageClient_UsesV1BaseUrlWithTrailingSlashByDefault()
    {
        var services = new ServiceCollection();
        services.AddVoyageClient(o => o.ApiKey = "test-key");

        using var provider = services.BuildServiceProvider();
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

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IVoyageClient>();

        var httpClient = GetHttpClient(client);
        Assert.Equal(new Uri("https://example.com/api/v1/"), httpClient.BaseAddress);
    }

    [Fact]
    public void AddVoyageClient_SetsBearerAuthorizationHeader()
    {
        var services = new ServiceCollection();
        services.AddVoyageClient(o => o.ApiKey = "my-secret-key");

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IVoyageClient>();
        var httpClient = GetHttpClient(client);

        Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization?.Scheme);
        Assert.Equal("my-secret-key", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
    }

    [Fact]
    public void AddVoyageClient_RequestsHttp2WithHttp11Fallback()
    {
        var services = new ServiceCollection();
        services.AddVoyageClient(o => o.ApiKey = "test-key");

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IVoyageClient>();
        var httpClient = GetHttpClient(client);

        Assert.Equal(HttpVersion.Version20, httpClient.DefaultRequestVersion);
        Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, httpClient.DefaultVersionPolicy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddVoyageClient_RejectsMissingApiKey(string? apiKey)
    {
        var services = new ServiceCollection();
        services.AddVoyageClient(o => o.ApiKey = apiKey!);
        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IVoyageClient>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("relative/path")]
    [InlineData("file:///tmp/voyage")]
    [InlineData("https://example.com/v1?key=value")]
    [InlineData("https://example.com/v1#fragment")]
    public void AddVoyageClient_RejectsInvalidBaseUrl(string? baseUrl)
    {
        var services = new ServiceCollection();
        services.AddVoyageClient(o =>
        {
            o.ApiKey = "test-key";
            o.BaseUrl = baseUrl is null ? null! : new Uri(baseUrl, UriKind.RelativeOrAbsolute);
        });
        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IVoyageClient>());
    }

    private static HttpClient GetHttpClient(IVoyageClient client)
    {
        var field = typeof(VoyageClient).GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<HttpClient>(field.GetValue(client));
    }
}
