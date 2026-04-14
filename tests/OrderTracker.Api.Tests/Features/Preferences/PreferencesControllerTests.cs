using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderTracker.Api.Features.Preferences;
using OrderTracker.Api.Features.Preferences.Dtos;
using Xunit;

namespace OrderTracker.Api.Tests.Features.Preferences;

public sealed class PreferencesControllerTests
{
    private static PreferencesController CreateController(Dictionary<string, string>? cookies = null)
    {
        var controller = new PreferencesController();
        var httpContext = new DefaultHttpContext();

        if (cookies is { Count: > 0 })
        {
            var cookieHeader = string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));
            httpContext.Request.Headers["Cookie"] = cookieHeader;
        }

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public void GetTheme_ReturnsLight_WhenNoCookiePresent()
    {
        var controller = CreateController();

        var result = controller.GetTheme();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<GetThemeResponse>()
            .Which.Theme.Should().Be("light");
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public void GetTheme_ReturnsCookieValue_WhenCookiePresent(string theme)
    {
        var controller = CreateController(new Dictionary<string, string> { { "theme", theme } });

        var result = controller.GetTheme();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<GetThemeResponse>()
            .Which.Theme.Should().Be(theme);
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public void SetTheme_ReturnsNoContent_ForValidTheme(string theme)
    {
        var controller = CreateController();

        var result = controller.SetTheme(new SetThemeRequest(theme));

        result.Should().BeOfType<NoContentResult>();
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public void SetTheme_SetsCookieInResponse(string theme)
    {
        var controller = CreateController();

        controller.SetTheme(new SetThemeRequest(theme));

        var setCookie = controller.HttpContext.Response.Headers["Set-Cookie"].ToString();
        setCookie.Should().Contain($"theme={theme}");
        setCookie.Should().Contain("path=/");
        setCookie.Should().Contain("max-age=");
        setCookie.Should().NotContainEquivalentOf("HttpOnly");
    }

    [Fact]
    public void SetTheme_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Theme", "Theme must be 'light' or 'dark'.");

        var result = controller.SetTheme(new SetThemeRequest("invalid"));

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
