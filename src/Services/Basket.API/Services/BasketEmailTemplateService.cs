using Basket.API.Services.Interfaces;

namespace Basket.API.Services;

public class BasketEmailTemplateService : EmailTemplateService, IEmailTemplateService
{
    public string GenerateReminderCheckoutOrderEmail(string email, string userName)
    {
        var checkOutUrl = "http//localhost:5000/baskets/checkout";
        var emailText = ReadEmailTemplateContent("remider-checkout-order");
        var emailReplacedText = emailText.Replace("[userName]", userName).Replace("[checkoutUrl]", checkOutUrl);
        
        return emailReplacedText;
    }
}