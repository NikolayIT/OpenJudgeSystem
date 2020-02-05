namespace OJS.Web.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Expressions;
    using System.Web.Routing;
    using System.Web.Script.Serialization;

    using MvcThrottle;

    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Cache;
    using OJS.Services.Data.Contests;
    using OJS.Web.Common;
    using OJS.Web.Common.Extensions;

    using static OJS.Common.GlobalConstants;

    // TODO: handle setting ViewBag data throught the help of this attribute
    // [PopulateMainContestCategoriesIntoViewBag]
    // [EnableThrottling]
    public class BaseController : Controller
    {
        public BaseController(IOjsData data) => this.Data = data;

        protected BaseController(IOjsData data, UserProfile profile)
            : this(data) => this.UserProfile = profile;

        protected IOjsData Data { get; set; }

        protected UserProfile UserProfile { get; set; }

        protected internal RedirectToRouteResult RedirectToAction<TController>(Expression<Action<TController>> expression)
            where TController : Controller
        {
            if (expression.Body is MethodCallExpression method)
            {
                return this.RedirectToAction(method.Method.Name);
            }

            throw new ArgumentException("Expected method call");
        }

        protected override void Initialize(RequestContext requestContext)
        {
            SetUiCultureFromCookie(requestContext);

            base.Initialize(requestContext);
        }

        protected ActionResult RedirectToHome() =>
            this.RedirectToAction<HomeController>(c => c.Index(), new { area = string.Empty });

        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            // Work with data before BeginExecute to prevent "NotSupportedException: A second operation started on this context before a previous asynchronous operation completed."
            this.UserProfile = this.Data.Users.GetByUsername(requestContext.HttpContext.User.Identity.Name);

            // Implement setting the MainContestCategories with action filter
            var cacheItems = ObjectFactory.GetInstance<ICacheItemsProviderService>();

            this.ViewBag.MainCategories = cacheItems.GetMainContestCategories();

            // Calling BeginExecute before PrepareSystemMessages for the TempData to has values
            var result = base.BeginExecute(requestContext, callback, state);

            var systemMessages = this.PrepareSystemMessages();
            this.ViewBag.SystemMessages = systemMessages;

            return result;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            if (this.Request.IsAjaxRequest())
            {
                var exception = filterContext.Exception as HttpException;

                if (exception != null)
                {
                    this.Response.StatusCode = exception.GetHttpCode();
                    this.Response.StatusDescription = exception.Message;
                }
            }
            else
            {
                var controllerName = this.ControllerContext.RouteData.Values["Controller"].ToString();
                var actionName = this.ControllerContext.RouteData.Values["Action"].ToString();
                this.View("Error", new HandleErrorInfo(filterContext.Exception, controllerName, actionName)).ExecuteResult(this.ControllerContext);
            }

            filterContext.ExceptionHandled = true;
        }

        /// <summary>
        /// Creates a JSON object with maximum size.
        /// </summary>
        /// <param name="data">JSON data.</param>
        /// <returns>Returns a JSON as content result.</returns>
        protected ContentResult LargeJson(object data)
        {
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 };

            return new ContentResult
            {
                Content = serializer.Serialize(data),
                ContentType = GlobalConstants.JsonMimeType,
            };
        }

        protected bool CheckIfUserHasContestPermissions(int contestId)
        {
            var contestsData = ObjectFactory.GetInstance<IContestsDataService>();

            return this.UserProfile != null &&
                (this.User.IsAdmin() ||
                    contestsData.IsUserLecturerInByContestAndUser(contestId, this.UserProfile.Id));
        }

        protected bool CheckIfUserHasProblemPermissions(int problemId) =>
            this.UserProfile != null &&
            (this.User.IsAdmin() ||
                this.Data.Problems
                    .All()
                    .Any(x =>
                        x.Id == problemId &&
                        (x.ProblemGroup.Contest.Lecturers.Any(y => y.LecturerId == this.UserProfile.Id) ||
                        x.ProblemGroup.Contest.Category.Lecturers.Any(cl => cl.LecturerId == this.UserProfile.Id))));

        protected bool CheckIfUserHasContestCategoryPermissions(int categoryId) =>
            this.UserProfile != null &&
            (this.User.IsAdmin() ||
                this.Data.ContestCategories
                    .All()
                    .Any(x =>
                        x.Id == categoryId &&
                        x.Lecturers.Any(y => y.LecturerId == this.UserProfile.Id)));

        protected bool CheckIfUserOwnsSubmission(int submissionId) =>
            this.UserProfile != null &&
            this.Data.Submissions
                .All()
                .Any(s => s.Id == submissionId && s.Participant.UserId == this.UserProfile.Id);

        private static void SetUiCultureFromCookie(RequestContext requestContext)
        {
            var languageCookie = requestContext.HttpContext.Request.Cookies[GlobalConstants.LanguageCookieName];

            if (languageCookie == null)
            {
                return;
            }

            switch (languageCookie.Value)
            {
                case GlobalConstants.BulgarianCultureCookieValue:
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(GlobalConstants.BulgarianCultureInfoName);
                    break;
                case GlobalConstants.EnglishCultureCookieValue:
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(GlobalConstants.EnglishCultureInfoName);
                    break;
                default: return;
            }
        }

        private SystemMessageCollection PrepareSystemMessages()
        {
            // Warning: always escape data to prevent XSS
            var messages = new SystemMessageCollection();

            if (this.TempData.ContainsKey(GlobalConstants.InfoMessage))
            {
                messages.Add(this.TempData[GlobalConstants.InfoMessage].ToString(), SystemMessageType.Success, 1000);
            }

            if (this.TempData.ContainsKey(GlobalConstants.DangerMessage))
            {
                messages.Add(this.TempData[GlobalConstants.DangerMessage].ToString(), SystemMessageType.Error, 1000);
            }

            if (this.UserProfile != null)
            {
                if (this.UserProfile.PasswordHash == null)
                {
                    messages.Add(Resources.Base.Main.Password_not_set, SystemMessageType.Warning, 0);
                }
            }

            return messages;
        }
    }
}