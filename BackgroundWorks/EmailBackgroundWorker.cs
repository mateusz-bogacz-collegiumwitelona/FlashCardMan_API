using Events.Emails.Helpers;
using Events.Emails.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.Mail;

namespace BackgroundWorks
{
    public class EmailBackgroundWorker : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly IEmailQueue _queue;
        public EmailBackgroundWorker(IConfiguration config, IEmailQueue queue)
        {
            _config = config;
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = await _queue.DequeueAsync(stoppingToken);

                    await SendEmailAsync(message);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in EmailBackgroundWorker: {ex.Message}");
                }
            }

        }

        private async Task SendEmailAsync(EmailMessage message)
        {
            try {
                var host = _config["Mail:Host"];
                var port = int.TryParse(_config["Mail:Port"], out var p) ? p : 1025;
                var enableSsl = bool.TryParse(_config["Mail:EnableSsl"], out var ssl) && ssl;
                var fromEmail = _config["Mail:From"];
                var displayName = _config["Mail:DisplayName"] ?? "DEV";

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(fromEmail))
                {
                    Console.WriteLine($"Email configuration missing. Cannot send email to {message.To}");
                    return;
                }

                using var mail = new MailMessage()
                {
                    From = new MailAddress(fromEmail, displayName),
                    To = { new MailAddress(message.To) },
                    Subject = message.Subject,
                    Body = message.Body,
                    IsBodyHtml = true
                };

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    UseDefaultCredentials = false,
                    Credentials = null,
                    Timeout = 10000
                };

                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email to {message.To}. Error: {ex.Message} | {ex.InnerException}");
            }
        }
    }
}
