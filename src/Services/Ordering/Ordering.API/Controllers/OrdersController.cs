using System.ComponentModel.DataAnnotations;
using System.Net;
using Contracts.Services;
using MailKit.Net.Smtp;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Models;
using Ordering.Application.Features.V1.Orders;
using Ordering.Domain.Entities;
using Shared.Services.Email;

namespace Ordering.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISmtpEmailService _emailService;
    
    public OrdersController(IMediator mediator, ISmtpEmailService emailService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    private static class RouteNames
    {
        public const string GetOrders = nameof(GetOrders);
    }

    [HttpGet("{userName}", Name = RouteNames.GetOrders)]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByUserName([Required] string userName)
    {
        var query = new GetOrdersQuery(userName);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet("test-email")]
    public async Task<ActionResult> TestEmail()
    {
        var message = new MailRequest
        {
            Body = "<h1>Hello World!</h1>",
            Subject = "Test mail",
            ToAddress = "leduyngan186@gmail.com",
        };
        await _emailService.SendEmailAsync(message);
        return Ok();
    }
}