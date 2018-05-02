namespace OJS.Web.Common.ActionResults
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using OJS.Common;
    using OJS.Web.Common.Models;

    internal class StandardJsonResult : JsonResult
    {
        private const string JsonReuqestGetBehaviorNotAllowedErrorMessage =
            "GET access is not allowed. Change the JsonRequestBehavior if you need GET access.";

        private readonly ICollection<string> errorMessages = new List<string>();

        public void AddErrorMessage(string message)
        {
            this.errorMessages.Add(message);
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
                throw new InvalidOperationException(JsonReuqestGetBehaviorNotAllowedErrorMessage);
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
            if (this.errorMessages.Any())
            {
                this.Data = new JsonResponse
                {
                    OriginalData = this.Data,
                    ErrorMessages = this.errorMessages
                };

                response.TrySkipIisCustomErrors = true;
            }

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