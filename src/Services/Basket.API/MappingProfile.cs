using AutoMapper;
using Basket.API.Entities;
using EventBus.Messages.IntergrationEvents.Events;

namespace Basket.API;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<BasketCheckout, BasketCheckOutEvent>().ReverseMap();
    }
}