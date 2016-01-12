namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common.Models;
    using OJS.Data;

    public class RedirectsController : BaseController
    {
        public RedirectsController(IOjsData data)
            : base(data)
        {
        }

        public static List<KeyValuePair<string, string>> OldSystemRedirects { get; } = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Contest/List", "/Contests"),
                    new KeyValuePair<string, string>("Home/SubmissionLog", "/Submissions"),
                    new KeyValuePair<string, string>("Home/ReportBug", "/Feedback"),
                    new KeyValuePair<string, string>("Home/SendBugReport", "/Feedback"),
                    new KeyValuePair<string, string>("Account/LogOn", "/Account/Login"),
                    new KeyValuePair<string, string>("Account/Profile", "/Users/Profile"),
                };

        public RedirectResult Index(int id)
        {
            return this.RedirectPermanent(OldSystemRedirects[id].Value);
        }

        public RedirectResult ProfileView(int id)
        {
            var username = this.Data.Users.All().Where(x => x.OldId == id).Select(x => x.UserName).FirstOrDefault();
            return this.RedirectPermanent($"/Users/{username}");
        }

        public RedirectResult ContestCompete(int id)
        {
            var newId = this.Data.Contests.All().Where(x => x.OldId == id).Select(x => x.Id).FirstOrDefault();
            return this.RedirectPermanent($"/Contests/Compete/Index/{newId}");
        }

        public RedirectResult ContestPractice(int id)
        {
            var newId = this.Data.Contests.All().Where(x => x.OldId == id).Select(x => x.Id).FirstOrDefault();
            return this.RedirectPermanent($"/Contests/Practice/Index/{newId}");
        }

        public RedirectResult ContestResults(int id)
        {
            var newId = this.Data.Contests.All().Where(x => x.OldId == id).Select(x => x.Id).FirstOrDefault();
            return this.RedirectPermanent($"/Contests/Compete/Results/Simple/{newId}");
        }

        public RedirectResult PracticeResults(int id)
        {
            var newId = this.Data.Contests.All().Where(x => x.OldId == id).Select(x => x.Id).FirstOrDefault();
            return this.RedirectPermanent($"/Contests/Practice/Results/Simple/{newId}");
        }

        public RedirectResult DownloadTask(int id)
        {
            var resourceId =
                this.Data.Resources.All()
                    .Where(x => x.Problem.OldId == id && x.Type == ProblemResourceType.ProblemDescription)
                    .Select(x => x.Id)
                    .FirstOrDefault();
            return this.RedirectPermanent($"/Contests/Practice/DownloadResource/{resourceId}");
        }
    }
}