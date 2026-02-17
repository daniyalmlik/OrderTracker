using System.ComponentModel.DataAnnotations;

namespace OrderTracker.Api.Features.Auth.Dtos;

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);
