namespace OJS.Tools.SendMailToAllUsers
{
    using System;
    using System.Collections.Generic;

    using OJS.Common;

    internal class Program
    {
        internal static void Main()
        {
            IEnumerable<string> mails = new List<string> { "nikolay.kostov@telerik.com", "admin@nikolay.it" };
            MailSender.Instance.SendMail("bgcoder.com@gmail.com", "кирилица", "<b>никииииииии!!!</b>васко...", mails);
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}
