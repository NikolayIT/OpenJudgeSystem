namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    using HtmlAgilityPack;

    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Controllers;

    using DatabaseModelType = OJS.Data.Models.News;
    using Resource = Resources.News;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.News.NewsAdministrationViewModel;

    public class NewsController : KendoGridAdministrationController
    {
        private const string InfoManUrl = "http://infoman.musala.com/feeds/";
        private const string InfoManEncoding = "utf-8";
        private const string InfosUrl = "http://www.math.bas.bg/infos/index.html";
        private const string InfosEncoding = "windows-1251";

        public NewsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.News.All()
                .Where(news => !news.IsDeleted)
                .Select(ViewModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.News
                .All()
                .FirstOrDefault(o => o.Id == (int)id);
        }

        public override string GetEntityKeyName()
        {
            return this.GetEntityKeyNameByType(typeof(DatabaseModelType));
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var id = this.BaseCreate(model.GetEntityModel());
            model.Id = (int)id;
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var entity = this.GetById(model.Id) as DatabaseModelType;
            this.BaseUpdate(model.GetEntityModel(entity));
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            this.BaseDestroy(model.Id);
            return this.GridOperation(request, model);
        }

        public ActionResult Fetch()
        {
            var fetchedNews = new List<News>();

            fetchedNews.AddRange(this.FetchNewsFromInfos());
            fetchedNews.AddRange(this.FetchNewsFromInfoMan());

            this.PopulateDatabaseWithNews(fetchedNews);

            this.TempData["InfoMessage"] = Resource.Views.All.News_successfully_added;
            return this.RedirectToAction("All", "News", new { Area = string.Empty });
        }

        private void PopulateDatabaseWithNews(IEnumerable<News> fetchedNews)
        {
            foreach (var news in fetchedNews)
            {
                if (!string.IsNullOrEmpty(news.Title) && !string.IsNullOrEmpty(news.Content) && news.Content.Length > 10 && !this.Data.News.All().Any(existingNews => existingNews.Title == news.Title))
                {
                    this.Data.News.Add(news);
                }

                this.Data.SaveChanges();
            }
        }

        // TODO: Refactor! This code should ne in two separate classes (for each site) and each class should implement an interface
        private HtmlDocument GetHtmlDocument(string url, string encoding)
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

        private IEnumerable<News> FetchNewsFromInfoMan()
        {
            var document = this.GetHtmlDocument(InfoManUrl, InfoManEncoding);

            var nodes = document.DocumentNode.SelectNodes("//rss//channel//item");

            var currentListOfNews = new List<News>();

            foreach (var node in nodes)
            {
                var title = node.ChildNodes.First(n => n.Name == "title").InnerHtml;
                var description = node.ChildNodes.First(n => n.Name == "description").InnerHtml;
                var date = node.ChildNodes.First(n => n.Name == "pubdate").InnerHtml;

                var decodedDescription = HttpUtility.HtmlDecode(description);
                var parsedDate = DateTime.Parse(date);

                currentListOfNews.Add(new News
                {
                    Title = title,
                    Content = this.ConvertLinks(decodedDescription, "http://infoman.musala.com/"),
                    Author = "ИнфоМан",
                    Source = "http://infoman.musala.com/",
                    IsVisible = true,
                    CreatedOn = parsedDate,
                    PreserveCreatedOn = true,
                });
            }

            return currentListOfNews;
        }

        private IEnumerable<News> FetchNewsFromInfos()
        {
            var document = this.GetHtmlDocument(InfosUrl, InfosEncoding);

            var currentListOfNews = new List<News>();

            HtmlNode node = document.DocumentNode.SelectSingleNode("//body//div//div//div[4]");

            this.GenerateNewsFromInfos(node, currentListOfNews);

            return currentListOfNews;
        }

        private void GenerateNewsFromInfos(HtmlNode node, List<News> fetchedNews)
        {
            var title = string.Empty;
            DateTime date = DateTime.Now;
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
                    date = this.TryGetDate(node.PreviousSibling.PreviousSibling.FirstChild.InnerText);
                    node = node.NextSibling;
                    continue;
                }
                else if (node.FirstChild.Attributes.Any(x => x.Name == "class" && x.Value == "ws14") && content.Length == 0)
                {
                    title += node.FirstChild.InnerText + " ";
                }
                else if (node.FirstChild.Attributes.Any(x => x.Name == "class" && x.Value == "ws14") && content.Length > 0)
                {
                    date = this.TryGetDate(content.ToString().Substring(0, 10));
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

        // TODO: May be moved to string extensions and unit test it
        private DateTime TryGetDate(string date)
        {
            try
            {
                return DateTime.ParseExact(date, "dd/MM/yyyy", null);
            }
            catch (Exception)
            {
                return new DateTime(2010, 1, 1);
            }
        }

        private string ConvertLinks(string content, string newLink)
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
    }
}