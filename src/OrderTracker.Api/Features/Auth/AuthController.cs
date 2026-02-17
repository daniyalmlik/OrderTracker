using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrderTracker.Api.Configuration;
using OrderTracker.Api.Features.Auth.Dtos;
using OrderTracker.Api.Features.Auth.Services;

namespace OrderTracker.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IOptions<JwtSettings> jwtOptions) : ControllerBase
{
    private const string RefreshCookieName = "refreshToken";
    private readonly int _refreshTokenExpiryDays = jwtOptions.Value.RefreshTokenExpiryDays;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.RegisterAsync(request, cancellationToken);
            SetRefreshCookie(result.RefreshToken);
            return Ok(new AuthResponse(result.AccessToken, result.ExpiresAt));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.LoginAsync(request, cancellationToken);
            SetRefreshCookie(result.RefreshToken);
            return Ok(new AuthResponse(result.AccessToken, result.ExpiresAt));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "No refresh token provided." });

        try
        {
            var result = await authService.RefreshAsync(refreshToken, cancellationToken);
            SetRefreshCookie(result.RefreshToken);
            return Ok(new AuthResponse(result.AccessToken, result.ExpiresAt));
        }
        catch (UnauthorizedAccessException)
        {
            Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/auth" });
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
            await authService.LogoutAsync(refreshToken, cancellationToken);

        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/auth" });
        return NoContent();
    }

    private void SetRefreshCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            MaxAge = TimeSpan.FromDays(_refreshTokenExpiryDays)
        });
    }
}
