namespace OJS.Web.Common
{
    using System.Web.Mvc;

    using Ionic.Zip;

    using OJS.Common;

    /// <summary>
    /// A content result which can accepts a DotNetZip ZipFile object to write to the output stream
    /// As seen on http://whatschrisdoing.com/blog/2010/03/22/asp-net-mvc-and-ionic-zip/
    /// </summary>
    public class ZipFileResult : ActionResult
    {
        private readonly ZipFile zip;

        private readonly string filename;

        public ZipFileResult(ZipFile zip, string filename)
        {
            this.zip = zip;
            this.filename = filename;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;

            response.ContentType = GlobalConstants.ZipMimeType;
            response.AddHeader(
                "Content-Disposition",
                "attachment;" + (string.IsNullOrEmpty(this.filename) ? string.Empty : "filename=" + this.filename));

            this.zip.Save(response.OutputStream);

            response.End();
        }
    }
}
