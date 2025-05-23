using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Basket.API.Services;
using Basket.API.Services.Interfaces;
using Contracts.Domains.Interfaces;
using Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs.ScheduledJob;
using ILogger = Serilog.ILogger;

namespace Basket.API.Repositories;

public class BasketRepository : IBasketRepository
{
    private readonly IDistributedCache _redisCacheService;
    private readonly ISerializeService _serializeService;
    private readonly ILogger _logger;
    private readonly BackgroundJobHttpService _backgroundJobHttp;
    private readonly IEmailTemplateService _emailTemplateService;

    public BasketRepository(IDistributedCache redisCacheService, ILogger logger, ISerializeService serializeService, BackgroundJobHttpService backgroundJobHttp, IEmailTemplateService emailTemplateService)
    {
        _redisCacheService = redisCacheService;
        _logger = logger;
        _serializeService = serializeService;
        _backgroundJobHttp = backgroundJobHttp;
        _emailTemplateService = emailTemplateService;
    }
    
    public async Task<Cart?> GetBasketByUserName(string username)
    {
        _logger.Information($"BEGIN: GetBasketByUserName {username}");
        var basket = await _redisCacheService.GetStringAsync(username);
        
        if (!string.IsNullOrEmpty(basket))
        {
            var result = _serializeService.Deserialize<Cart>(basket);
            var totalPrice = result.TotalPrice;
            _logger.Information("END: GetBasketByUserName {username} - Total Price: {totalPrice}", username, totalPrice);
            return result;
        }
        
        return string.IsNullOrEmpty(basket) ? null : _serializeService.Deserialize<Cart>(basket);
    }

    public async Task<Cart> UpdateBasket(Cart cart, DistributedCacheEntryOptions options = null)
    {
        DeleteReminderCheckoutOrder(cart.Username);
        _logger.Information($"BEGIN: UpdateBasket for {cart.Username}");
        if (options != null)
        {
            await _redisCacheService.SetStringAsync(cart.Username, _serializeService.Serialize(cart), options);
        }
        else
        {
            await _redisCacheService.SetStringAsync(cart.Username, _serializeService.Serialize(cart));
        }
        _logger.Information($"END: UpdateBasket for {cart.Username}");

        try
        {
            await TriggerSendEmailReminderCheckout(cart);
        }
        catch (Exception e)
        {
            _logger.Error($"UpdateBasket send mail failed: {e.Message}");
        }
        return await GetBasketByUserName(cart.Username);
    }

    private async Task DeleteReminderCheckoutOrder(string username)
    {
        var cart = await GetBasketByUserName(username);
        if(cart == null || string.IsNullOrEmpty(cart.JobId)) return;
        
        var jobId = cart.JobId;
        _backgroundJobHttp.DeleteReminderCheckoutOrder(jobId);
        _logger.Information($"DeleteReminderCheckoutOrder: Deleted jobId {jobId}");
    }

    private async Task TriggerSendEmailReminderCheckout(Cart cart)
    {
        var emailTemplate = _emailTemplateService.GenerateReminderCheckoutOrderEmail(cart.Username);
        
        var model = new ReminderCheckoutOrderDto(cart.EmailAddress, "Reminder Checkout", emailTemplate, DateTimeOffset.UtcNow.AddSeconds(30) );
        
        var jobId = await _backgroundJobHttp.SendEmailReminderCheckout(model);
        if (!string.IsNullOrEmpty(jobId))
        {
            cart.JobId = jobId;
            await _redisCacheService.SetStringAsync(cart.Username, _serializeService.Serialize(cart));
        }
    }

    public async Task<bool> DeleteBasketFromUserName(string username)
    {
        DeleteReminderCheckoutOrder(username);
        try
        {
            _logger.Information($"BEGIN: DeleteBasketFromUserName {username}");
           await _redisCacheService.RemoveAsync(username);  
           _logger.Information($"END: DeleteBasketFromUserName {username}");
           return true;
        }
        catch (Exception e)
        {
           _logger.Error("DeleteBasketFromUserName: " + e.Message);
            throw;
        }
    }
}