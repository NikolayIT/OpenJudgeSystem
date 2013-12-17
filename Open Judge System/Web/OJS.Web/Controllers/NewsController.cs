namespace OJS.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    using HtmlAgilityPack;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.ViewModels.News;

    using Resource = Resources.News;

    public class NewsController : BaseController
    {
        private const string InfoManUrl = "http://infoman.musala.com/welcome/main.php";
        private const string InfosUrl = "http://www.math.bas.bg/infos/index.html";

        public NewsController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult All(int id = 1, int pageSize = 10)
        {
            var newsCount = this.Data.News.All().Count(x => x.IsVisible);

            IEnumerable<NewsViewModel> news;
            int page = 0;
            int pages = 0;

            if (newsCount == 0)
            {
                news = new List<NewsViewModel>();
            }
            else
            {
                if (newsCount % pageSize == 0)
                {
                    pages = newsCount / pageSize;
                }
                else
                {
                    pages = (newsCount / pageSize) + 1;
                }

                if (id < 1)
                {
                    id = 1;
                }
                else if (id > pages)
                {
                    id = pages;
                }

                if (pageSize < 1)
                {
                    pageSize = 10;
                }

                page = id;

                news = this.Data.News.All()
                    .Where(x => x.IsVisible)
                    .OrderByDescending(x => x.CreatedOn)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(NewsViewModel.FromNews)
                    .ToList();
            }

            var allNewsModel = new AllNewsViewModel
            {
                AllNews = news,
                CurrentPage = page,
                PageSize = pageSize,
                AllPages = pages
            };

            return this.View(allNewsModel);
        }

        public ActionResult Selected(int id = 1)
        {
            var currentNews = this.Data.News.GetById(id);

            if (currentNews == null || currentNews.IsDeleted)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Views.Selected.Invalid_news_id);
            }

            var previousNews = this.Data.News.All()
                .OrderByDescending(x => x.Id)
                .Where(x => x.Id < currentNews.Id && x.IsVisible && !x.IsDeleted)
                .FirstOrDefault();

            if (previousNews == null)
            {
                previousNews = this.Data.News.All()
                    .OrderByDescending(x => x.Id)
                    .Where(x => x.IsVisible && !x.IsDeleted)
                    .First();
            }

            var nextNews = this.Data.News.All()
                .OrderBy(x => x.Id)
                .Where(x => x.Id > currentNews.Id && x.IsVisible && !x.IsDeleted)
                .FirstOrDefault();

            if (nextNews == null)
            {
                nextNews = this.Data.News.All()
                    .OrderBy(x => x.Id)
                    .Where(x => x.IsVisible && !x.IsDeleted)
                    .First();
            }

            var newsContentViewModel = new SelectedNewsViewModel
            {
                Id = currentNews.Id,
                Title = currentNews.Title,
                Author = currentNews.Author,
                Source = currentNews.Source,
                TimeCreated = currentNews.CreatedOn,
                Content = currentNews.Content,
                PreviousId = previousNews.Id,
                NextId = nextNews.Id
            };

            return this.View(newsContentViewModel);
        }

        [ChildActionOnly]
        public ActionResult LatestNews(int newsCount = 5)
        {
            var latestNews =
                this.Data.News.All()
                    .OrderByDescending(x => x.CreatedOn)
                    .Where(x => x.IsVisible && !x.IsDeleted)
                    .Select(SelectedNewsViewModel.FromNews)
                    .Take(newsCount)
                    .ToList();

            return this.PartialView("_LatestNews", latestNews);
        }

        public ActionResult Fetch()
        {
            var fetchedNews = new List<News>();

            fetchedNews.AddRange(this.FetchNewsFromInfos());

            // TODO: Rework fetching news from infoman.musala.com (they changed their site)
            // fetchedNews.AddRange(this.FetchNewsFromInfoMan());
            this.PopulateDatabaseWithNews(fetchedNews);

            this.TempData["InfoMessage"] = Resource.Views.All.News_successfully_added;
            return this.RedirectToAction("All");
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
        #region Fetching news
        private HtmlDocument GetHtmlDocument(string url)
        {
            var document = new HtmlDocument();

            using (var client = new WebClient())
            {
                using (var stream = client.OpenRead(url))
                {
                    var reader = new StreamReader(stream, Encoding.GetEncoding("windows-1251"));
                    var html = reader.ReadToEnd();
                    document.LoadHtml(html);
                }
            }

            document.OptionFixNestedTags = true;

            return document;
        }

        private IEnumerable<News> FetchNewsFromInfoMan()
        {
            var document = this.GetHtmlDocument(InfoManUrl);

            HtmlNode node = document.DocumentNode.SelectSingleNode("//body//table//tr[3]//td");

            var currentListOfNews = new List<News>();

            this.GenerateNewsFromInfoMan(node, currentListOfNews);

            return currentListOfNews;
        }

        private IEnumerable<News> FetchNewsFromInfos()
        {
            var document = this.GetHtmlDocument(InfosUrl);

            var currentListOfNews = new List<News>();

            HtmlNode node = document.DocumentNode.SelectSingleNode("//body//div//div//div[6]");

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

                    fetchedNews.Add(new News
                    {
                        Title = title.Trim(),
                        CreatedOn = date,
                        IsVisible = true,
                        Author = "Инфос",
                        Source = "http://www.math.bas.bg/infos/index.html",
                        Content = content.ToString().Trim().Substring(10),
                        PreserveCreatedOn = true,
                    });

                    title = string.Empty;
                    date = DateTime.Now;
                    content.Length = 0;
                }
                else if (node.FirstChild.Attributes.Any(x => x.Name == "class" && x.Value == "ws12"))
                {
                    content.Append(node.FirstChild.InnerHtml);
                }

                node = node.NextSibling;
            }
        }

        private void GenerateNewsFromInfoMan(HtmlNode node, List<News> fetchedNews)
        {
            try
            {
                var title = string.Empty;
                var date = string.Empty;
                var content = new StringBuilder();

                foreach (var child in node.ChildNodes)
                {
                    if (child.ChildNodes.Any(tag => tag.Name == "h5"))
                    {
                        this.GenerateNewsFromInfoMan(child, fetchedNews);
                    }

                    if (child.Name == "h5" && title == string.Empty)
                    {
                        if (child.ChildNodes.Any(tag => tag.Attributes.Any(attr => attr.Name == "class" && attr.Value == "date")))
                        {
                            if (child.FirstChild == null || child.FirstChild.NextSibling == null || child.FirstChild.NextSibling.NextSibling == null)
                            {
                                continue;
                            }

                            fetchedNews.Add(new News
                                {
                                    Title = child.FirstChild.InnerText.Trim(),
                                    CreatedOn = this.TryGetDate(child.FirstChild.NextSibling.InnerText),
                                    IsVisible = true,
                                    Author = "ИнфоМан",
                                    Source = "http://infoman.musala.com/",
                                    Content = child.FirstChild.NextSibling.NextSibling.InnerHtml.Trim(),
                                    PreserveCreatedOn = true,
                                });

                            continue;
                        }

                        if (child.FirstChild == null || child.NextSibling == null || child.NextSibling.NextSibling == null)
                        {
                            break;
                        }

                        title = child.FirstChild.InnerText;
                        date = child.NextSibling.NextSibling.InnerText;
                    }
                    else if (child.Name == "h5" && title != string.Empty && content.Length != 0)
                    {
                        fetchedNews.Add(new News
                        {
                            Title = title.Trim(),
                            CreatedOn = this.TryGetDate(date),
                            IsVisible = true,
                            Author = "ИнфоМан",
                            Source = "http://infoman.musala.com/",
                            Content = content.ToString().Trim(),
                            PreserveCreatedOn = true,
                        });

                        if (child.FirstChild == null || child.NextSibling == null || child.NextSibling.NextSibling == null)
                        {
                            break;
                        }

                        title = child.FirstChild.InnerText;
                        date = child.NextSibling.NextSibling.InnerText;
                        content.Length = 0;
                    }
                    else if (child.Name == "p" && child.Attributes.Count == 0)
                    {
                        content.AppendLine(child.InnerHtml);
                    }
                }

                if (content.Length != 0)
                {
                    fetchedNews.Add(new News
                    {
                        Title = title.Trim(),
                        CreatedOn = this.TryGetDate(date),
                        IsVisible = true,
                        Author = "ИнфоМан",
                        Source = "http://infoman.musala.com/",
                        Content = content.ToString().Trim(),
                        PreserveCreatedOn = true,
                    });
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion

        // TODO: May be move to string extensions and unit test it
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
    }
}