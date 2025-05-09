using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using Shared.DTOs.Basket;
using EventBus.Messages.IntegrationEvents.Events;
using Inventory.Product.API.Services.Interfaces;
using Shared.DTOs.Inventory;

namespace Basket.API.IntegrationEvents;

public class UpdateInventoryConsumer : IConsumer<UpdateInventoryEvent>
{
    private readonly IInventoryService _inventoryService;

    public UpdateInventoryConsumer( IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task Consume(ConsumeContext<UpdateInventoryEvent> context)
    {
        var documentNo = await _inventoryService.SalesOrderAsync(context.Message.SalesOrder);
        var result = new CreatedSalesOrderSuccessDto(documentNo);
        
        await context.Publish(new InventoryUpdatedEvent
        {
            CorrelationId = context.Message.CorrelationId,
            DocumentNo = result.DocumentNo,
            Success = result.DocumentNo != null
        });
    }
}