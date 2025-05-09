using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using Shared.DTOs.Basket;
using EventBus.Messages.IntegrationEvents.Events;
using Inventory.Product.API.Services.Interfaces;
using Shared.DTOs.Inventory;

namespace Basket.API.IntegrationEvents;

public class DeleteInventoryConsumer : IConsumer<DeleteInventoryEvent>
{
    private readonly IInventoryService _inventoryService;

    public DeleteInventoryConsumer( IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task Consume(ConsumeContext<DeleteInventoryEvent> context)
    {
       var result = await _inventoryService.DeleteByDocumentNoAsync(context.Message.DocumentNo);
        
        await context.Publish(new InventoryDeletedEvent
        {
            CorrelationId = context.Message.CorrelationId,
            Success = result
        });
    }
}