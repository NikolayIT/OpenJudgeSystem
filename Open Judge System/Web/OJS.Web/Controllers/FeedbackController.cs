namespace OJS.Web.Controllers
{
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.ViewModels.Feedback;

    using Recaptcha;

    using Resource = Resources.Feedback.Views;

    public class FeedbackController : BaseController
    {
        public FeedbackController(IOjsData data)
            : base(data)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        [RecaptchaControlMvc.CaptchaValidator]
        public ActionResult Index(FeedbackViewModel model, bool captchaValid)
        {
            if (!captchaValid)
            {
                this.ModelState.AddModelError("Captcha", Resource.FeedbackIndex.Invalid_captcha);
            }

            if (this.ModelState.IsValid)
            {
                var report = new FeedbackReport
                {
                    Content = model.Content,
                    Email = model.Email,
                    Name = model.Name
                };

                if (this.User.Identity.IsAuthenticated)
                {
                    var userProfile = this.Data.Users.GetByUsername(this.User.Identity.Name);
                    report.User = userProfile;
                }

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
