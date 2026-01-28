using Events.Emails.Helpers;

namespace Events.Emails.Interfaces
{
    public interface IEmailQueue
    {
        void QueueEmail(string to, string subject, string body);
        ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken);
    }
}
