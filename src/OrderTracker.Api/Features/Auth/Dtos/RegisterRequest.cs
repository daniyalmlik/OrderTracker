using System.ComponentModel.DataAnnotations;

namespace OrderTracker.Api.Features.Auth.Dtos;

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(128)] string Password,
    [Required, MaxLength(100)] string FullName
);
