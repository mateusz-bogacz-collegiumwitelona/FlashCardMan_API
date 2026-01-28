namespace Events.Emails.Helpers
{
    public record EmailMessage(string To, string Subject, string Body);
}
