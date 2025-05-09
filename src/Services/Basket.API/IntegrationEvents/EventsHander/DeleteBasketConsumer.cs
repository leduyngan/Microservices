using System.Threading.Tasks;
using AutoMapper;
using Basket.API.Repositories.Interfaces;
using MassTransit;
using Shared.DTOs.Basket;
using EventBus.Messages.IntegrationEvents.Events;

namespace Basket.API.IntegrationEvents;

public class DeleteBasketConsumer : IConsumer<DeleteBasketCommand>
{
    private readonly IBasketRepository _basketHttpRepository;

    public DeleteBasketConsumer(IBasketRepository basketHttpRepository)
    {
        _basketHttpRepository = basketHttpRepository;
    }

    public async Task Consume(ConsumeContext<DeleteBasketCommand> context)
    {
        var result = await _basketHttpRepository.DeleteBasketFromUserName(context.Message.UserName);
        await context.Publish(new BasketDeletedEvent
        {
            CorrelationId = context.Message.CorrelationId,
            Success = result
        });
    }
}