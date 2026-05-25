using E_Commerce.DTOs.Requests;
using E_Commerce.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

[Authorize]
public class OrdersController : BaseController
{
    private readonly IOrderServices _orderServices;

    public OrdersController(IOrderServices orderServices)
    {
        _orderServices = orderServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderHistory()
    {
        var result = await _orderServices.GetOrderHistory(GetUserId());
        return ToResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var result = await _orderServices.GetOrderById(id, GetUserId());
        return ToResponse(result);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CreateOrderRequest request)
    {
        var result = await _orderServices.Checkout(GetUserId(), request);
        return ToResponse(result);
    }

    [Authorize(Roles = "Admin,SalesManager")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await _orderServices.UpdateOrderStatus(id, request);
        return ToResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var result = await _orderServices.CancelOrder(id, GetUserId());
        return ToResponse(result);
    }
}
