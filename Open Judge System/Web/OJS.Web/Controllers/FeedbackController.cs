namespace OJS.Web.Controllers
{
    using System.Web.Mvc;

    using OJS.Common;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Common.Attributes;
    using OJS.Web.ViewModels.Feedback;
    using Resource = Resources.Feedback.Views;

    [Authorize]
    public class FeedbackController : BaseController
    {
        protected const int RequestsPerInterval = 3;

        // Restrict interval is 5 minutes.
        protected const int RestrictInterval = 300;

        public FeedbackController(IOjsData data)
            : base(data)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            var inputViewModel = new FeedbackViewModel
            {
                Name = $"{this.UserProfile.UserSettings.FirstName} {this.UserProfile.UserSettings.LastName}".Trim(),
                Email = this.UserProfile.Email
            };
            return this.View(inputViewModel);
        }

        [HttpPost]
        [RestrictRequests(
            RequestsPerInterval = RequestsPerInterval,
            RestrictInterval = RestrictInterval,
            ErrorMessage = "Прекалено много заявки. Моля, опитайте по-късно.")]
        public ActionResult Index(FeedbackViewModel model, bool? captchaValid = null)
        {
            if (this.ModelState.IsValid)
            {
                var report = new FeedbackReport
                {
                    Content = model.Content,
                    Email = model.Email,
                    Name = model.Name,
                    UserId = this.UserProfile.Id
                };

                this.Data.FeedbackReports.Add(report);
                this.Data.SaveChanges();

                this.TempData[GlobalConstants.InfoMessage] = Resource.FeedbackIndex.Feedback_submitted;
                return this.RedirectToAction("Submitted");
            }

            return this.View(model);
        }

        [HttpGet]
        public ActionResult Submitted()
        {
            return this.View();
        }
    }
}
