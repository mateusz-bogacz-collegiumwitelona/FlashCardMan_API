using Events.Emails.Interfaces;
using System.Threading.Channels;

namespace Events.Emails.Helpers
{
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<(string To, string Subject, string Body)> _queue;

        public EmailQueue()
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<(string To, string Subject, string Body)>(options);
        }

        public void QueueEmail(string to, string subject, string body)
            => _queue.Writer.TryWrite((to, subject, body));

        public async ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
        {
            var (to, subject, body) = await _queue.Reader.ReadAsync(cancellationToken);
            return new EmailMessage(to, subject, body);
        }
    }
}
