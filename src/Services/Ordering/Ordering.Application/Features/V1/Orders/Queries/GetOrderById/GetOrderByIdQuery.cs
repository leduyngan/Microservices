using MediatR;
using Ordering.Application.Common.Models;
using Ordering.Domain.Entities;
using Shared.SeedWork;

namespace Ordering.Application.Features.V1.Orders.Queries.GetOrderById;

public class GetOrderByIdQuery : IRequest<ApiResult<OrderDto>>
{
    public long Id { get; private set; }

    public GetOrderByIdQuery(long id)
    {
        Id = id;
    }
}