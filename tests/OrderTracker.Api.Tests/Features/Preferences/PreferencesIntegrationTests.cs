using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderTracker.Api.Data;
using Microsoft.Extensions.Configuration;
using OrderTracker.Api.Infrastructure.Kafka;
using Xunit;

namespace OrderTracker.Api.Tests.Features.Preferences;

public sealed class PreferencesIntegrationTests : IClassFixture<PreferencesWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PreferencesIntegrationTests(PreferencesWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTheme_ReturnsLight_WhenNoCookieSet()
    {
        var response = await _client.GetAsync("/api/preferences/theme");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ThemeDto>();
        body!.Theme.Should().Be("light");
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public async Task PostTheme_ReturnsNoContent_ForValidTheme(string theme)
    {
        var response = await _client.PostAsJsonAsync("/api/preferences/theme", new { theme });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public async Task PostTheme_SetsNonHttpOnlyCookie(string theme)
    {
        var response = await _client.PostAsJsonAsync("/api/preferences/theme", new { theme });

        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var setCookie = string.Join("; ", cookies!);
        setCookie.Should().Contain($"theme={theme}");
        setCookie.Should().Contain("path=/");
        setCookie.Should().Contain("max-age=");
        setCookie.Should().NotContainEquivalentOf("httponly");
    }

    [Fact]
    public async Task PostTheme_ReturnsBadRequest_ForInvalidTheme()
    {
        var response = await _client.PostAsJsonAsync("/api/preferences/theme", new { theme = "blue" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTheme_ReturnsDark_WhenCookieSetToDark()
    {
        // Set the cookie first
        var postResponse = await _client.PostAsJsonAsync("/api/preferences/theme", new { theme = "dark" });
        postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Extract and replay the cookie
        postResponse.Headers.TryGetValues("Set-Cookie", out var setCookieValues);
        var themeCookie = setCookieValues!
            .First(c => c.StartsWith("theme="))
            .Split(';')[0]; // "theme=dark"

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/preferences/theme");
        request.Headers.Add("Cookie", themeCookie);

        var getResponse = await _client.SendAsync(request);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadFromJsonAsync<ThemeDto>();
        body!.Theme.Should().Be("dark");
    }

    private sealed record ThemeDto(string Theme);
}

public sealed class PreferencesWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Provide minimal valid config so Program.cs startup guards don't throw
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:SecretKey"] = "test-secret-key-at-least-32-characters-long!",
                ["JwtSettings:AccessTokenExpiryMinutes"] = "15",
                ["JwtSettings:RefreshTokenExpiryDays"] = "7",
                ["Kafka:BootstrapServers"] = "localhost:9092",
                ["Kafka:Topic"] = "order-events",
                ["Kafka:ConsumerGroup"] = "test-group",
                ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=TestDb;Trusted_Connection=True;"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace SQL Server with InMemory DB
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // Replace Kafka producer with a no-op stub
            services.RemoveAll<IKafkaProducerService>();
            services.AddSingleton<IKafkaProducerService, NoOpKafkaProducerService>();

            // Remove the background consumer service (would fail without real Kafka)
            services.RemoveAll<Microsoft.Extensions.Hosting.IHostedService>();

            // Replace JWT Bearer options — appsettings.json has an empty SecretKey which throws
            // IDX10703 before PostConfigure ever runs. Remove the original IConfigureOptions
            // registrations and replace with one that uses a valid test key.
            var jwtConfigDescriptors = services
                .Where(d => d.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>))
                .ToList();
            foreach (var d in jwtConfigDescriptors) services.Remove(d);

            services.AddSingleton<IConfigureOptions<JwtBearerOptions>>(
                new ConfigureNamedOptions<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes("test-secret-key-at-least-32-characters-long!"))
                        };
                    }));
        });
    }
}

internal sealed class NoOpKafkaProducerService : IKafkaProducerService
{
    public Task PublishAsync(KafkaEventPayload payload, CancellationToken ct = default) => Task.CompletedTask;
}
