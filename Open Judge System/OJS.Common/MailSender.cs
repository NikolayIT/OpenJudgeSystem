namespace OJS.Common
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Mail;
    using System.Text;

    public sealed class MailSender
    {
        // TODO: Extract user, address and password as app.config settings
        private const string SendFrom = "bgcoder.com@gmail.com";
        private const string SendFromName = "BGCoder.com";

        private const string SmtpUsername = "SMTPUser";
        private const string Password = "M@1L^unRestricteD!";

        private const string ServerAddress = "smtp.softuni.bg";
        private const int ServerPort = 25;

        private static readonly object SyncRoot = new object();

        private static MailSender instance;
        private readonly SmtpClient mailClient;

        private MailSender()
        {
            this.mailClient = new SmtpClient
            {
                Credentials = new NetworkCredential(SmtpUsername, Password),
                Port = ServerPort,
                Host = ServerAddress,
                EnableSsl = true,
            };

            ////this.mailClient = new SmtpClient { DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis };
        }

        public static MailSender Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new MailSender();
                        }
                    }
                }

                return instance;
            }
        }

        public void SendMailAsync(string recipient, string subject, string messageBody, IEnumerable<string> bccRecipients = null)
        {
            var message = this.PrepareMessage(recipient, subject, messageBody, bccRecipients);
            this.mailClient.SendAsync(message, null);
        }

        public void SendMail(string recipient, string subject, string messageBody, IEnumerable<string> bccRecipients = null)
        {
            var message = this.PrepareMessage(recipient, subject, messageBody, bccRecipients);
            this.mailClient.Send(message);
        }

        private MailMessage PrepareMessage(string recipient, string subject, string messageBody, IEnumerable<string> bccRecipients)
        {
            var mailTo = new MailAddress(recipient);
            var mailFrom = new MailAddress(SendFrom, SendFromName);
            var message = new MailMessage(mailFrom, mailTo)
            {
                Body = messageBody,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
            };

            if (bccRecipients != null)
            {
                foreach (var bccRecipient in bccRecipients)
                {
                    message.Bcc.Add(bccRecipient);
                }
            }

            return message;
        }
    }
}
