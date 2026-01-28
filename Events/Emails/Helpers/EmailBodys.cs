using System.Text;

namespace Events.Emails.Helpers
{
    public class EmailBodys
    {
        public string GenerateRegisterConfirmEmailBody(string userName, string token)
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html>");
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("<meta charset='UTF-8'>");
            sb.Append("<style>");
            sb.Append("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.Append(".container { max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; border: 1px solid #ddd; border-radius: 8px; }");
            sb.Append(".header { background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }");
            sb.Append(".content { background-color: white; padding: 30px; border-radius: 0 0 8px 8px; }");
            sb.Append(".info-box { background-color: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0; }");
            sb.Append(".button { display: inline-block; padding: 15px 30px; margin: 20px 0; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }");
            sb.Append(".button:hover { background-color: #218838; }");
            sb.Append(".footer { text-align: center; margin-top: 20px; font-size: 12px; color: #666; }");
            sb.Append("</style>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append("<div class='container'>");
            sb.Append("<div class='header'>");
            sb.Append("<h1>Welcome to Fuel App!</h1>");
            sb.Append("</div>");
            sb.Append("<div class='content'>");
            sb.Append($"<p>Dear <strong>{userName}</strong>,</p>");
            sb.Append("<p>Thank you for registering! We're excited to have you on board.</p>");
            sb.Append("<p>To complete your registration and activate your account, please confirm your email address by clicking the button below:</p>");
            sb.Append("<div style='text-align: center;'>");
            sb.Append("</div>");
            sb.Append("<p>If the button doesn't work, copy and paste this link into your browser:</p>");
            sb.Append($"<p style='word-break: break-all; color: #28a745;'>{token}</p>");
            sb.Append("<p>If you didn't create an account with us, please ignore this email.</p>");
            sb.Append("<p>Best regards,<br><strong>Fuel App Team</strong></p>");
            sb.Append("</div>");
            sb.Append("<div class='footer'>");
            sb.Append("<p>This is an automated message. Please do not reply to this email.</p>");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }

        public string GenerateResetPasswordBody(string userName, string token)
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html>");
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("<meta charset='UTF-8'>");
            sb.Append("<style>");
            sb.Append("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.Append(".container { max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; border: 1px solid #ddd; border-radius: 8px; }");
            sb.Append(".header { background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }");
            sb.Append(".content { background-color: white; padding: 30px; border-radius: 0 0 8px 8px; }");
            sb.Append(".info-box { background-color: #d1ecf1; border-left: 4px solid #007bff; padding: 15px; margin: 20px 0; }");
            sb.Append(".button { display: inline-block; padding: 15px 30px; margin: 20px 0; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }");
            sb.Append(".button:hover { background-color: #0056b3; }");
            sb.Append(".footer { text-align: center; margin-top: 20px; font-size: 12px; color: #666; }");
            sb.Append("</style>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append("<div class='container'>");
            sb.Append("<div class='header'>");
            sb.Append("<h1>Password Reset Request</h1>");
            sb.Append("</div>");
            sb.Append("<div class='content'>");
            sb.Append($"<p>Dear <strong>{userName}</strong>,</p>");
            sb.Append("<p>We received a request to reset your password. Click the button below to create a new password:</p>");
            sb.Append("<div style='text-align: center;'>");
            sb.Append("<p>If the button doesn't work, copy and paste this link into your browser:</p>");
            sb.Append($"<p style='word-break: break-all; color: #007bff;'>{token}</p>");
            sb.Append("<p>Best regards,<br><strong>Fuel App Security Team</strong></p>");
            sb.Append("</div>");
            sb.Append("<div class='footer'>");
            sb.Append("<p>This is an automated message. Please do not reply to this email.</p>");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }

    }
}
