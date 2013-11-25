namespace OJS.Web.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Script.Serialization;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Common;

    public class BaseController : Controller
    {
        public BaseController(IOjsData data)
        {
            this.Data = data;
        }

        public BaseController(IOjsData data, UserProfile profile)
            : this(data)
        {
            this.UserProfile = profile;
        }

        protected IOjsData Data { get; set; }

        protected UserProfile UserProfile { get; set; }

        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            var result = base.BeginExecute(requestContext, callback, state);

            this.UserProfile = this.Data.Users.GetByUsername(requestContext.HttpContext.User.Identity.Name);

            var systemMessages = this.PrepareSystemMessages();
            ViewBag.SystemMessages = systemMessages;

            return result;
        }

        private SystemMessageCollection PrepareSystemMessages()
        {
            // Warning: always escape data to prevent XSS
            var messages = new SystemMessageCollection();
            
            if (TempData.ContainsKey("InfoMessage"))
            {
                messages.Add(this.TempData["InfoMessage"].ToString(), SystemMessageType.Success, 1000);
            }

            if (TempData.ContainsKey("DangerMessage"))
            {
                messages.Add(this.TempData["DangerMessage"].ToString(), SystemMessageType.Error, 1000);
            }

            if (this.UserProfile != null)
            {
                if (this.UserProfile.PasswordHash == null)
                {
                    messages.Add("Нямате парола за вход в сайта. Моля сложете си парола от <a href=\"/Account/Manage\">този линк</a>.", SystemMessageType.Warning, 0);
                }

            }

            return messages;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            if (Request.IsAjaxRequest())
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
                var controllerName = ControllerContext.RouteData.Values["Controller"].ToString();
                var actionName = ControllerContext.RouteData.Values["Action"].ToString();
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
                ContentType = "application/json",
            };
        }
    }
}