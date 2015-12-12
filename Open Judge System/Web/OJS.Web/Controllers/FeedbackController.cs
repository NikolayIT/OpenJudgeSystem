namespace OJS.Web.Controllers
{
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Web.Common.Attributes;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.ViewModels.Feedback;
    using Resource = Resources.Feedback.Views;

    [Authorize]
    public class FeedbackController : BaseController
    {
        protected const int RequestsPerInterval = 3;
        protected const int RestrictInterval = 300;

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
        ////[RecaptchaControlMvc.CaptchaValidator]
        [RestrictRequests(
            RequestsPerInterval = RequestsPerInterval,
            RestrictInterval = RestrictInterval,
            ErrorMessage = "Прекалено много заявки. Моля, опитайте по-късно.")]
        public ActionResult Index(FeedbackViewModel model, bool? captchaValid = null)
        {
            ////if (!captchaValid)
            ////{
            ////    this.ModelState.AddModelError("Captcha", Resource.FeedbackIndex.Invalid_captcha);
            ////}

            if (this.ModelState.IsValid)
            {
                var report = new FeedbackReport
                {
                    Content = model.Content,
                    Email = model.Email,
                    Name = model.Name,
                    User = this.Data.Users.GetByUsername(User.Identity.Name)
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
