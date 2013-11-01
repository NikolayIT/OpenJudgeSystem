namespace OJS.Web.Controllers
{
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Data.Models;

    public class FeedbackController : BaseController
    {
        private const string FeedbackSubmitMessage = "Благодарим ви за обратната връзка. Ще се постараем да поправим проблема възможно най-скоро!";

        public FeedbackController(IOjsData data)
            : base(data)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            return this.View(new FeedbackReport());
        }

        [HttpPost]
        public ActionResult Index(FeedbackReport feedback)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userProfile = this.Data.Users.GetByUsername(User.Identity.Name);
                    feedback.User = userProfile;
                }

                this.Data.FeedbackReports.Add(feedback);
                this.Data.SaveChanges();

                return this.RedirectToAction("Submitted");
            }

            return this.View(feedback);
        }
        
        [HttpGet]
        public ActionResult Submitted()
        {
            TempData["InfoMessage"] = FeedbackSubmitMessage;
            return this.View();
        }
    }
}
