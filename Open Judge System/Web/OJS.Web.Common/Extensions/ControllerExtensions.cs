namespace OJS.Web.Common.Extensions
{
    using System.Text;
    using System.Web.Mvc;

    using OJS.Web.Common.ActionResults;

    public static class ControllerExtensions
    {
        public static ActionResult JsonSuccess(
            this Controller controller,
            object data,
            string contentType = null,
            Encoding contentEncoding = null,
            JsonRequestBehavior jsonRequestBehavior = JsonRequestBehavior.AllowGet)
        {
            var result = new StandardJsonResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = jsonRequestBehavior
            };

            if (data is string message)
            {
               result.AddSuccessMessage(message);
            }

            return result;
        }

        public static JsonResult JsonError(
            this Controller controller,
            string errorMessage,
            string contentType = null,
            Encoding contentEncoding = null,
            JsonRequestBehavior jsonRequestBehavior = JsonRequestBehavior.AllowGet)
        {
            var result = new StandardJsonResult
            {
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = jsonRequestBehavior,
            };

            result.AddErrorMessage(errorMessage);

            return result;
        }
    }
}