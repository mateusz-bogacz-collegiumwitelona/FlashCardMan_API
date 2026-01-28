using Events.Emails.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Events.Emails.Helpers
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ICollection<string> _frontendURL;
        private readonly IEmailQueue _emailQueue;
        private readonly EmailBodys _emailBodys;

        public EmailSender(
            IConfiguration config,
            IEmailQueue emailQueue,
            EmailBodys emailBodys
            )
        {
            _config = config;
            _frontendURL = new List<string>
            {
                $"{_config["Frontend:DevURL"] ?? "http://localhost:3000"}"
            };
            _emailQueue = emailQueue;
            _emailBodys = emailBodys;
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            _emailQueue.QueueEmail(toEmail, subject, body);
            Console.WriteLine($"Email queued to {toEmail} with subject {subject}");
            return await Task.FromResult(true);
        }

        public async Task SendRegisterEmailAsync(string email, string userName, string token)
        {
            try 
            {
                string encodedToken = Uri.EscapeDataString(token);
                string encodedEmail = Uri.EscapeDataString(email);

                var emailBody = _emailBodys.GenerateRegisterConfirmEmailBody(
                    userName,
                    token
                    );

                string subject = "Confirm Your Registration";

                await SendEmailAsync(email, subject, emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending registration email to {email}: {ex.Message} | {ex.InnerException}");
            }
        }

        public async Task SendResetPasswordEmailAsync(
           string email,
           string userName,
           string token
           )
        {
            try
            {
                string encodedToken = Uri.EscapeDataString(token);
                string encodedEmail = Uri.EscapeDataString(email);
               

                var emailBody = _emailBodys.GenerateResetPasswordBody(userName, encodedToken);
                string subject = "Confirm Reset Passowrd";

                await SendEmailAsync(email, subject, emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendRegisterConfirmEmailAsync for {email}: {ex.Message} | {ex.InnerException}");
            }
        }
    }
}
