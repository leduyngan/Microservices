using Infrastructure.Extensions;
using Shared.Configurations;
using Shared.DTOs.ScheduledJob;

namespace Basket.API.Services;

public class BackgroundJobHttpService
{
    private readonly HttpClient _client;

    private readonly string _scheduleJobUrl;

    public BackgroundJobHttpService(HttpClient client, BackgroundJobSettings settings)
    {
        client.BaseAddress = new Uri(settings.HangfireUrl);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        _client = client;
        _scheduleJobUrl = settings.ScheduleJobUrl;
    }

    public async Task<string> SendEmailReminderCheckout(ReminderCheckoutOrderDto model)
    {
        var uri = $"{_scheduleJobUrl}/send-reminder-checkout-order-email";
        var response = await _client.PostAsJson(uri, model);
        string jobId = null;
        if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
            jobId = await response.ReadContentAs<string>();
        
        return jobId;
    }

    public void DeleteReminderCheckoutOrder(string jobId)
    {
        var uri = $"{_scheduleJobUrl}/delete/jobId/{jobId}";
        _client.DeleteAsync(uri);
    }
    
}