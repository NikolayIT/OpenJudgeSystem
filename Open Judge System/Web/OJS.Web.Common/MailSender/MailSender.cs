namespace OJS.Web.Common.MailSender
{
    using System;

    public class MailSender
    {
        private static MailSender instance;

        private MailSender()
        {
        }

        public static MailSender Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MailSender();
                }

                return instance;
            }
        }

        public void SendMail(string recipient, string title, string message)
        {
            throw new NotImplementedException();
        }
    }
}
