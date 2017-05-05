namespace OJS.Common.Extensions
{
    using System.IO;
    using System.Web;

    public static class HttpPostedFileBaseExtensions
    {
        public static byte[] ToByteArray(this HttpPostedFileBase httpPostedFileBaseToConvert)
        {
            using (var archiveStream = new MemoryStream())
            {
                httpPostedFileBaseToConvert.InputStream.CopyTo(archiveStream);
                return archiveStream.ToArray();
            }
        }
    }
}
