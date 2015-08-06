namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.ViewModels.News;

    using Resource = Resources.News;

    public class NewsController : BaseController
    {
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

            var previousNews = this.Data.News
                .All()
                .OrderByDescending(x => x.Id)
                .FirstOrDefault(x => x.Id < currentNews.Id && x.IsVisible && !x.IsDeleted);

            if (previousNews == null)
            {
                previousNews = this.Data.News
                    .All()
                    .OrderByDescending(x => x.Id)
                    .First(x => x.IsVisible && !x.IsDeleted);
            }

            var nextNews = this.Data.News
                .All()
                .OrderBy(x => x.Id)
                .FirstOrDefault(x => x.Id > currentNews.Id && x.IsVisible && !x.IsDeleted);

            if (nextNews == null)
            {
                nextNews = this.Data.News
                    .All()
                    .OrderBy(x => x.Id)
                    .First(x => x.IsVisible && !x.IsDeleted);
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
    }
}