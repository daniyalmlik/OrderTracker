namespace OrderTracker.Api.Features.Auth.Services;

/// <summary>Internal result returned by AuthService. The refresh token is extracted by the
/// controller to set an HTTP-only cookie; only the access token is sent to the client.</summary>
public sealed record AuthResult(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken
);
