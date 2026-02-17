using OrderTracker.Api.Domain.Enums;

namespace OrderTracker.Api.Domain.Entities;

public sealed class Order
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public required string ItemName { get; init; }
    public int Quantity { get; init; } = 1;
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; init; } = null!;
    public ICollection<OrderEvent> Events { get; init; } = [];
}
