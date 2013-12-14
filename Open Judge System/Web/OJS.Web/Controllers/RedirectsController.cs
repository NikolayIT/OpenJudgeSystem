namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common.Models;
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
                new KeyValuePair<string, string>("Account/Profile", "/Users/Profile"),
            };

        public RedirectsController(IOjsData data)
            : base(data)
        {
        }

        public RedirectResult Index(int id)
        {
            return this.RedirectPermanent(OldSystemRedirects[id].Value);
        }

        public RedirectResult ProfileView(int id)
        {
            var username = this.Data.Users.All().Where(x => x.OldId == id).Select(x => x.UserName).FirstOrDefault();
            return this.RedirectPermanent(string.Format("/Users/{0}", username));
        }

        public RedirectResult ContestCompete(int id)
        {
            var newId = this.Data.Contests.All().Where(x => x.OldId == id).Select(x => x.Id).FirstOrDefault();
            return this.RedirectPermanent(string.Format("/Contests/Compete/Index/{0}", newId));
        }

        public RedirectResult ContestPractice(int id)
        {
            var newId = this.Data.Contests.All().Where(x => x.OldId == id).Select(x => x.Id).FirstOrDefault();
            return this.RedirectPermanent(string.Format("/Contests/Practice/Index/{0}", newId));
        }

        public RedirectResult ContestResults(int id)
        {
            var newId = this.Data.Contests.All().Where(x => x.OldId == id).Select(x => x.Id).FirstOrDefault();
            return this.RedirectPermanent(string.Format("/Contests/Compete/Results/Simple/{0}", newId));
        }

        public RedirectResult PracticeResults(int id)
        {
            var newId = this.Data.Contests.All().Where(x => x.OldId == id).Select(x => x.Id).FirstOrDefault();
            return this.RedirectPermanent(string.Format("/Contests/Practice/Results/Simple/{0}", newId));
        }

        public RedirectResult DownloadTask(int id)
        {
            var resourceId =
                this.Data.Resources.All()
                    .Where(x => x.Problem.OldId == id && x.Type == ProblemResourceType.ProblemDescription)
                    .Select(x => x.Id)
                    .FirstOrDefault();
            return this.RedirectPermanent(string.Format("/Contests/Practice/DownloadResource/{0}", resourceId));
        }
    }
}