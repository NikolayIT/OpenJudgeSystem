namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using OJS.Data;

    public class RedirectsController : BaseController
    {
        public static List<KeyValuePair<string, string>> OldSystemRedirects =
            new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Contest/List", "/Contests"),
                new KeyValuePair<string, string>("Home/SubmissionLog", "/Submissions"),
                new KeyValuePair<string, string>("Home/ReportBug", "/Feedback"),
                new KeyValuePair<string, string>("Home/SendBugReport", "/Feedback"),
                new KeyValuePair<string, string>("Account/LogOn", "/Account/Login"),
            };

        public RedirectsController(IOjsData data)
            : base(data)
        {
        }

        public RedirectResult Index(int id)
        {
            return RedirectPermanent(OldSystemRedirects[id].Value);
        }
    }
}