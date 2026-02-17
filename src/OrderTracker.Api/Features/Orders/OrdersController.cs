using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderTracker.Api.Features.Orders.Dtos;
using OrderTracker.Api.Features.Orders.Services;

namespace OrderTracker.Api.Features.Orders;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> List(CancellationToken cancellationToken)
    {
        var orders = await orderService.ListUserOrdersAsync(CurrentUserId, cancellationToken);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.CreateOrderAsync(CurrentUserId, request, cancellationToken);
            return CreatedAtAction(nameof(GetEvents), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.UpdateOrderStatusAsync(id, CurrentUserId, request, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        try
        {
            await orderService.CancelOrderAsync(id, CurrentUserId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/events")]
    public async Task<ActionResult<IReadOnlyList<OrderEventDto>>> GetEvents(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var events = await orderService.GetOrderEventsAsync(id, CurrentUserId, cancellationToken);
            return Ok(events);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
