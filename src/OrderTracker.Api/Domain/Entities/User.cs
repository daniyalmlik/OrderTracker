namespace OrderTracker.Api.Domain.Entities;

public sealed class User
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; init; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; init; } = [];
}
