using Microsoft.AspNetCore.Mvc;
using OrderTracker.Api.Features.Preferences.Dtos;

namespace OrderTracker.Api.Features.Preferences;

[ApiController]
[Route("api/preferences")]
public sealed class PreferencesController : ControllerBase
{
    private const string ThemeCookieName = "theme";

    [HttpGet("theme")]
    public ActionResult<GetThemeResponse> GetTheme()
    {
        var theme = Request.Cookies[ThemeCookieName] ?? "light";
        return Ok(new GetThemeResponse(theme));
    }

    [HttpPost("theme")]
    public IActionResult SetTheme([FromBody] SetThemeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid theme value." });

        Response.Cookies.Append(ThemeCookieName, request.Theme, new CookieOptions
        {
            HttpOnly = false,            // Intentional: JS-accessible (contrasts with auth cookies)
            Secure = false,              // Dev only
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromDays(365)
        });

        return NoContent();
    }
}
