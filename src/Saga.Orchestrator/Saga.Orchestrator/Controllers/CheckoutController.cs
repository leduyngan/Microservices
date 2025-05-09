using System.ComponentModel.DataAnnotations;
using Contracts.Sagas.OrderManager;
using Microsoft.AspNetCore.Mvc;
using Saga.Orchestrator.OrderManager;
using Saga.Orchestrator.Services.Interfaces;
using Shared.DTOs.Basket;

namespace Saga.Orchestrator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly ISagaOrderManager<BasketCheckoutDto, OrderResponse> _sagaOrderManager;
    private readonly ISagaOrderManagerRabbitMq<BasketCheckoutDto, OrderResponse> _sagaOrderManagerRabbitMq;
    
    public CheckoutController( ISagaOrderManager<BasketCheckoutDto, OrderResponse> sagaOrderManager,ISagaOrderManagerRabbitMq<BasketCheckoutDto, OrderResponse> sagaOrderManagerRabbitMq)
    {
        _sagaOrderManager = sagaOrderManager;
        _sagaOrderManagerRabbitMq = sagaOrderManagerRabbitMq;
    }

    [HttpPost]
    [Route("{username}")]
    public async Task<OrderResponse> CheckoutOrder([Required] string username, [FromBody] BasketCheckoutDto model)
    {
        model.UserName = username;
        // var result = _sagaOrderManager.CreateOrder(model);
        var result = await _sagaOrderManagerRabbitMq.CreateOrder(model);
        return result;
    }
}


