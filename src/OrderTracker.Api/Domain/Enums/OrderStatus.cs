namespace OrderTracker.Api.Domain.Enums;

public enum OrderStatus
{
    Placed,
    Confirmed,
    Preparing,
    OutForDelivery,
    Delivered,
    Cancelled
}
