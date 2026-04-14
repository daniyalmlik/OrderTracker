using System.ComponentModel.DataAnnotations;

namespace OrderTracker.Api.Features.Preferences.Dtos;

public sealed record SetThemeRequest(
    [Required, RegularExpression("^(light|dark)$", ErrorMessage = "Theme must be 'light' or 'dark'.")]
    string Theme
);
