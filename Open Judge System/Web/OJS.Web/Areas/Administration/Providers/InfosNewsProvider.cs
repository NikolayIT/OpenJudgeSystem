namespace OJS.Web.Areas.Administration.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using HtmlAgilityPack;

    using OJS.Common.Extensions;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Providers.Common;

    public class InfosNewsProvider : BaseNewsProvider
    {
        private const string ContentUrl = "http://www.math.bas.bg/infos/index.html";
        private const string ContentEncoding = "windows-1251";

        public override IEnumerable<News> FetchNews()
        {
            var document = this.GetHtmlDocument(ContentUrl, ContentEncoding);

            var currentListOfNews = new List<News>();

            HtmlNode node = document.DocumentNode.SelectSingleNode("//body//div//div//div[4]");

            this.GenerateNewsFromInfos(node, currentListOfNews);

            return currentListOfNews;
        }

        private void GenerateNewsFromInfos(HtmlNode node, List<News> fetchedNews)
        {
            var title = string.Empty;
            var date = DateTime.Now;
            var content = new StringBuilder();

            while (true)
            {
                if (node == null)
                {
                    break;
                }

                if (node.FirstChild == null)
                {
                    node = node.NextSibling;
                    continue;
                }
                else if (node.FirstChild.InnerText == string.Empty && content.Length > 0)
                {
                    node = node.NextSibling;
                    continue;
                }
                else if (node.FirstChild.InnerText == string.Empty)
                {
                    date = node.PreviousSibling.PreviousSibling.FirstChild.InnerText.TryGetDate();
                    node = node.NextSibling;
                    continue;
                }
                else if (node.FirstChild.Attributes.Any(x => x.Name == "class" && x.Value == "ws14") && content.Length == 0)
                {
                    title += node.FirstChild.InnerText + " ";
                }
                else if (node.FirstChild.Attributes.Any(x => x.Name == "class" && x.Value == "ws14") && content.Length > 0)
                {
                    date = content.ToString().Substring(0, 10).TryGetDate();
                    string contentAsString = content.ToString().Trim().Substring(10);
                    contentAsString = this.ConvertLinks(contentAsString, "http://www.math.bas.bg/infos/");

                    fetchedNews.Add(new News
                    {
                        Title = title.Trim(),
                        CreatedOn = date,
                        IsVisible = true,
                        Author = "Инфос",
                        Source = "http://www.math.bas.bg/infos/index.html",
                        Content = contentAsString,
                        PreserveCreatedOn = true,
                    });

                    title = string.Empty;
                    date = DateTime.Now;
                    content.Length = 0;
                    continue;
                }
                else if (node.FirstChild.Attributes.Any(x => x.Name == "class" && x.Value == "ws12"))
                {
                    content.Append(node.FirstChild.InnerHtml);
                }

                node = node.NextSibling;
            }
        }
    }
}