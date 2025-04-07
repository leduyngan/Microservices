namespace Basket.API.Services.Interfaces;

public interface IEmailTemplateService
{
    string GenerateReminderCheckoutOrderEmail(string userName, string checkoutUrl = "baskets");
}