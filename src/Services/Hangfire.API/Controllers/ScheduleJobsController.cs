using System.ComponentModel.DataAnnotations;
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
    public async Task<IActionResult> SendReminderCheckoutOrderEmail([FromBody] ReminderCheckoutOrderDto model)
    {
        // await Task.Delay(10000); // simulate some delay for 10 seconds
        var jobId = _jobService.SendEmailContent(model.email, model.subject, model.emailContent, model.enqueueAt);
        return Ok(jobId);
    }

    [HttpDelete]
    [Route("delete/jobId/{id}")]
    public IActionResult DeleteJobId([Required] string id)
    {
        var result = _jobService.ScheduleJobService.Delete(id);
        return Ok(result);
    }
}