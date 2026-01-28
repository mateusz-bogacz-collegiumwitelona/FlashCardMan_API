using Events.Emails.Interfaces;
using Events.Event;
using Events.Interfaces;

namespace Events.Emails
{
    public class SendResetPasswordEmailHandler : IEventHandler<UserEvent>
    {
        private readonly IEmailSender _emailSender;
        public SendResetPasswordEmailHandler(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task HandleAsync(UserEvent @event)
            => await _emailSender.SendResetPasswordEmailAsync(
                @event.User.Email,
                @event.User.UserName,
                @event.ConfirmationToken
                );
    }
}
