using Events.Emails.Interfaces;
using Events.Event;
using Events.Interfaces;

namespace Events.Emails
{
    public class SendRegistrationEmailHandler : IEventHandler<UserEvent>
    {
        private readonly IEmailSender _emailSender;

        public SendRegistrationEmailHandler(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task HandleAsync(UserEvent @event)
            => await _emailSender.SendRegisterEmailAsync(
                @event.User.Email,
                @event.User.UserName,
                @event.ConfirmationToken
                );


    }
}

