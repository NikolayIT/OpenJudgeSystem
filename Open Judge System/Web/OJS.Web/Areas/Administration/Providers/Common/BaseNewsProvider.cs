namespace OJS.Web.Areas.Administration.Providers.Common
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;

    using HtmlAgilityPack;

    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Providers.Contracts;

    public abstract class BaseNewsProvider : INewsProvider
    {
        public abstract IEnumerable<News> FetchNews();

        protected string ConvertLinks(string content, string newLink)
        {
            var result = new StringBuilder();

            for (int i = 0; i < content.Length; i++)
            {
                if (i + 6 < content.Length && content.Substring(i, 6) == "href=\"")
                {
                    result.Append("href=\"");
                    i += 6;
                    if (i + 4 < content.Length && content.Substring(i, 4) == "http")
                    {
                        i += 4;
                        result.Append("http");
                    }
                    else
                    {
                        result.Append(newLink);
                    }
                }

                result.Append(content[i]);
            }

            return result.ToString();
        }

        protected HtmlDocument GetHtmlDocument(string url, string encoding)
        {
            var document = new HtmlDocument();

            using (var client = new WebClient())
            {
                using (var stream = client.OpenRead(url))
                {
                    var reader = new StreamReader(stream, Encoding.GetEncoding(encoding));
                    var html = reader.ReadToEnd();
                    document.LoadHtml(html);
                }
            }

            document.OptionFixNestedTags = true;

            return document;
        }
    }
}