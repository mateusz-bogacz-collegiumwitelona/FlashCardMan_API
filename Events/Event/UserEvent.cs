using Data.Models;
using Events.Interfaces;

namespace Events.Event
{
    public class UserEvent : IEvent
    {
        public ApplicationUser User { get; }
        public string ConfirmationToken { get; }

        public UserEvent(ApplicationUser user, string token)
        {
            User = user;
            ConfirmationToken = token;
        }
    }
}
