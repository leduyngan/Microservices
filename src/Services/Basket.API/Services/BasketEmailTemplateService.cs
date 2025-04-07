using Basket.API.Services.Interfaces;
using Shared.Configurations;

namespace Basket.API.Services;

public class BasketEmailTemplateService : EmailTemplateService, IEmailTemplateService
{
    public BasketEmailTemplateService(BackgroundJobSettings settings) : base(settings)
    {
    }

    public string GenerateReminderCheckoutOrderEmail(string userName, string checkoutUrl = "basket/checkout")
    {
        var checkOutUrl = $"{BackgroundJobSettings.ApiGwUrl}/{checkoutUrl}/{userName}" ;
        var emailText = ReadEmailTemplateContent("remider-checkout-order");
        var emailReplacedText = emailText.Replace("[userName]", userName).Replace("[checkoutUrl]", checkOutUrl);
        
        return emailReplacedText;
    }
}