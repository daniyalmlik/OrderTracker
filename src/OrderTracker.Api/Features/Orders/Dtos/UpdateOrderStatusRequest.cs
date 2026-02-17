using System.ComponentModel.DataAnnotations;
using OrderTracker.Api.Domain.Enums;

namespace OrderTracker.Api.Features.Orders.Dtos;

public sealed record UpdateOrderStatusRequest(
    [Required] OrderStatus NewStatus);
