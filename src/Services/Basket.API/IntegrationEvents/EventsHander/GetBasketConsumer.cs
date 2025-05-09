using System.Threading.Tasks;
using AutoMapper;
using Basket.API.Repositories.Interfaces;
using MassTransit;
using Shared.DTOs.Basket;
using EventBus.Messages.IntegrationEvents.Events;

namespace Basket.API.IntegrationEvents;

public class GetBasketConsumer : IConsumer<GetBasketCommand>
{
    private readonly IBasketRepository _basketHttpRepository;
    private readonly IMapper _mapper;

    public GetBasketConsumer(IBasketRepository basketHttpRepository, IMapper mapper)
    {
        _basketHttpRepository = basketHttpRepository;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<GetBasketCommand> context)
    {
        var response = await _basketHttpRepository.GetBasketByUserName(context.Message.UserName);
        var cart = _mapper.Map<CartDto>(response)?? new CartDto(context.Message.UserName);
        await context.Publish(new BasketRetrievedEvent
        {
            CorrelationId = context.Message.CorrelationId,
            Cart = cart,
            Success = cart != null
        });
    }
}