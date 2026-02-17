namespace OrderTracker.Api.Features.Auth.Dtos;

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt
);
