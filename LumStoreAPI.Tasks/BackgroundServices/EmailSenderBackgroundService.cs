using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace LumStoreAPI.Tasks.BackgroundServices
{
    internal class EmailSenderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public EmailSenderBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DoWork();
                await Task.Delay(10000);
            }
        }

        private async Task DoWork()
        {
            using (var serviceScoped = _serviceScopeFactory.CreateScope())
            {
                var emailService = serviceScoped.ServiceProvider.GetRequiredService<IEmailService>();
                var eventLogService = serviceScoped.ServiceProvider.GetRequiredService<IEventLogService>();
                var emailQueues = await emailService.GetEmailQueuesAsync(30);
                var emailConfig = await emailService.GetConfigAsync();
                foreach (var emailQueue in emailQueues)
                {
                    var emailMessage = new EmailMessage()
                    {
                        EmailBcc = emailQueue.EmailBcc,
                        EmailBody = emailQueue.EmailBody,
                        EmailCc = emailQueue.EmailCc,
                        EmailFrom = emailQueue.EmailFrom,
                        EmailSubject = emailQueue.EmailSubject,
                        EmailTo = emailQueue.EmailTo,
                    };
                    if (emailQueue.Attachments != null && emailQueue.Attachments.Any())
                    {
                        var mediaService = serviceScoped.ServiceProvider.GetRequiredService<IMediaLibraryService>();
                        var mediaItems = await mediaService.GetMediaItemsAsync(emailQueue.Attachments.Select(x => Guid.Parse(x)).ToArray());
                        if (mediaItems != null && mediaItems.Any())
                        {
                            var semaphore = new SemaphoreSlim(5);
                            emailMessage.Attachments = await Task.WhenAll(mediaItems.Select(async x =>
                            {
                                await semaphore.WaitAsync();
                                try
                                {
                                    var bytes = await MediaLibraryHelper.GetMediaLibraryBytesAsync(x);
                                    return new EmailAttachment(x.FileName, new MemoryStream(bytes), x.FileID);
                                }
                                finally { semaphore.Release(); }
                            }));
                        }
                    }

                    try
                    {
                        await EmailHelper.SendMail(emailMessage, emailConfig);
                        await emailService.UpdateEmailStatusAsync(emailQueue.ItemID, EmailStatus.Success);
                    }
                    catch (Exception ex)
                    {
                        await emailService.UpdateEmailStatusAsync(emailQueue.ItemID, EmailStatus.Failed);
                        await eventLogService.LogException("EmailSender", "SendEmail", "", ex);
                    }
                }
            }
        }
    }
}
