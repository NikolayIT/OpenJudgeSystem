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
    using Newtonsoft.Json.Serialization;

    using OJS.Common;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Common.Models;

    internal class StandardJsonResult : JsonResult
    {
        private const string GetAccessDeniedErrorMessage =
            "GET access is not allowed. Change the JsonRequestBehavior if you need GET access.";

        private readonly ICollection<string> errorMessages = new List<string>();

        public void AddError(string errorMessage)
        {
            this.errorMessages.Add(errorMessage);
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
                throw new InvalidOperationException(GetAccessDeniedErrorMessage);
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
                var originalData = this.Data;
                this.Data = new JsonResponse
                {
                    OriginalData = originalData,
                    ErrorMessages = this.errorMessages
                };

                if (!response.IsError())
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

                response.TrySkipIisCustomErrors = true;
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new JsonConverter[] { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore,
            };

            var serializedData = JsonConvert.SerializeObject(this.Data, settings);
            response.Write(serializedData);
        }
    }
}