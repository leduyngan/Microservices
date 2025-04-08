using Contracts.ScheduleJobs;
using Contracts.Services;
using Hangfire.API.Services.Interfaces;
using Shared.Services.Email;
using ILogger = Serilog.ILogger;

namespace Hangfire.API.Services;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly IScheduleJobService _jobService;
    private readonly ISmtpEmailService _emailService;
    private readonly ILogger _logger;

    public BackgroundJobService(IScheduleJobService jobService, ISmtpEmailService emailService, ILogger logger)
    {
        _jobService = jobService;
        _emailService = emailService;
        _logger = logger;
    }

    public IScheduleJobService ScheduleJobService => _jobService;

    public string? SendEmailContent(string email, string subject, string emailContent, DateTimeOffset enqueueAt)
    {
        var emailRequest = new MailRequest
        {
            Subject = subject,
            ToAddress = email,
            Body = emailContent,
        };

        try
        {
            var jobId = _jobService.Schedule(() => _emailService.SendEmail(emailRequest), enqueueAt);
            
            _logger.Information($"Email sent to {emailRequest.ToAddress} with subject: {emailRequest.Subject} - JobId: {jobId}");
            return jobId;
        }
        catch (Exception ex)
        {
            _logger.Error($"failed due to an error with the email service: {ex.Message}");
        }
        
        return null;
    }
}