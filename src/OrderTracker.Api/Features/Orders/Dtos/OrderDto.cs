namespace OrderTracker.Api.Features.Orders.Dtos;

public sealed record OrderDto(
    int Id,
    int UserId,
    string ItemName,
    int Quantity,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
