using Contracts.ScheduleJobs;
using Microsoft.AspNetCore.Mvc;

namespace Hangfire.API.Controllers;

public class WelcomeController : ControllerBase
{
    private readonly IScheduleJobService _jobService;
    private readonly Serilog.ILogger _logger;

    public WelcomeController(IScheduleJobService jobService, Serilog.ILogger logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    [HttpPost]
    [Route("[action]")]
    public IActionResult Welcome()
    {
        var jobId = _jobService.Enqueue(() => ResponseWelcome("Welcome to Hangfire API"));
        return Ok($"Job ID: {jobId} - Enqueue Job");
    }
    
    [HttpPost]
    [Route("[action]")]
    public IActionResult DelayedWelcome()
    {
        var seconds = 5;
        
        var jobId = _jobService.Schedule(() => ResponseWelcome("Welcome to Hangfire API after 5 seconds"), TimeSpan.FromSeconds(seconds));
        return Ok($"Job ID: {jobId} - Delayed Job");
    }
    
    [HttpPost]
    [Route("[action]")]
    public IActionResult WelcomeAt()
    {
        var enqueueAt = DateTimeOffset.UtcNow.AddSeconds(5);
        
        var jobId = _jobService.Schedule(() => ResponseWelcome("Welcome to Hangfire API At 5 seconds"), enqueueAt);
        return Ok($"Job ID: {jobId} - Delayed Job");
    }
    
    [HttpPost]
    [Route("[action]")]
    public IActionResult ConfirmWelcome()
    {
        const int timeInSeconds = 5;
        
        var parentJobId = _jobService.Schedule(() => ResponseWelcome("Welcome before confirm wellcome"), TimeSpan.FromSeconds(timeInSeconds));
        
        var jobId = _jobService.ContinueQueueWith(parentJobId, () => ResponseWelcome("Welcome message is sent"));
        return Ok($"Job ID: {jobId} - Confirmed Welcome will be sent in {timeInSeconds} seconds");
    }
    
    [NonAction]
    public void ResponseWelcome(string text)
    {
        _logger.Information(text);
    }

}