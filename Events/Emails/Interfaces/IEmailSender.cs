namespace Events.Emails.Interfaces
{
    public interface IEmailSender
    {
        Task SendRegisterEmailAsync(string email, string userName, string token);
        Task SendResetPasswordEmailAsync(string email, string userName, string token);
    }
}
