using System.ComponentModel.DataAnnotations;

namespace OrderTracker.Api.Features.Orders.Dtos;

public sealed record CreateOrderRequest(
    [Required, MaxLength(200)] string ItemName,
    [Range(1, 1000)] int Quantity = 1);
