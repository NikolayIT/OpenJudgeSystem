namespace OJS.Services.Common.Emails
{
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading.Tasks;

    public interface IEmailSenderService
    {
        void SendEmail(
            string recipient,
            string subject,
            string body,
            IEnumerable<string> bccRecipients = null,
            AttachmentCollection attachments = null);

        Task SendEmailAsync(
            string recipient,
            string subject,
            string body,
            IEnumerable<string> bccRecipients = null);
    }
}