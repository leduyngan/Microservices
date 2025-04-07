using Hangfire.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.ScheduledJob;

namespace Hangfire.API.Controllers;

[ApiController]
[Route("api/schedule-jobs")]
public class ScheduleJobsController : ControllerBase
{
    private readonly IBackgroundJobService _jobService;
    public ScheduleJobsController(IBackgroundJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost]
    [Route("send-reminder-checkout-order-email")]
    public IActionResult SendReminderCheckoutOrderEmail([FromBody] ReminderCheckoutOrderDto model)
    {
        var jobId = _jobService.SendEmailContent(model.email, model.subject, model.emailContent, model.enqueueAt);
        return Ok(jobId);
    }
}