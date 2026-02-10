using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Infrastructure.Services;

public class EmailBackgroundService(
    IEmailQueue emailQueue, 
    IEmailService emailService,
    ILogger<EmailBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Email Background Service started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var message = await emailQueue.DequeueEmailAsync(ct);
                await emailService.SendEmailAsync(message);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing email queue.");
            }
        }
        
        logger.LogInformation("Email Background Service stopped.");
    }
}
