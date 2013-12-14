namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
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

        public RedirectResult UserProfile(int id)
        {
            var username = this.Data.Users.All().Where(x => x.OldId == id).Select(x => x.UserName).FirstOrDefault();
            return RedirectPermanent(string.Format("/Users/{0}", username));
        }
    }
}