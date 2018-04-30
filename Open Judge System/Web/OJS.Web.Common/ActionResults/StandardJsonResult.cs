namespace OJS.Web.Common.ActionResults
{
    using System;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using OJS.Common;
    using OJS.Web.Common.Models;

    internal class StandardJsonResult : JsonResult
    {
        private string errorMessage;
        private string successMessage;

        public void AddErrorMessage(string message)
        {
            this.errorMessage = message;
        }

        public void AddSuccessMessage(string message)
        {
            this.successMessage = message;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
                string.Equals(
                    context.HttpContext.Request.HttpMethod,
                    WebRequestMethods.Http.Get,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "GET access is not allowed. Change the JsonRequestBehavior if you need GET access.");
            }

            var response = context.HttpContext.Response;
            response.ContentType = string.IsNullOrEmpty(this.ContentType)
                ? GlobalConstants.JsonMimeType
                : this.ContentType;

            if (this.ContentEncoding != null)
            {
                response.ContentEncoding = this.ContentEncoding;
            }

            this.SerializeData(response);
        }

        protected virtual void SerializeData(HttpResponseBase response)
        {
            this.Data = new JsonResponse
            {
                Data = this.Data,
                SuccessMessage = this.successMessage,
                ErrorMessage = this.errorMessage
            };

            var settings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore
            };

            var serializedData = JsonConvert.SerializeObject(this.Data, settings);
            response.Write(serializedData);
        }
    }
}