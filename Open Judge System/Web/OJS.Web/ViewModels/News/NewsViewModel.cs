namespace OJS.Web.ViewModels.News
{
    using System;
    using System.Linq.Expressions;
    using NewsModel = OJS.Data.Models.News;

    public class NewsViewModel
    {
        public static Expression<Func<NewsModel, NewsViewModel>> FromNews
        {
            get
            {
                return news => new NewsViewModel
                {
                    Id = news.Id,
                    Title = news.Title,
                    Author = news.Author,
                    Source = news.Source,
                    TimeCreated = news.CreatedOn
                };
            }
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Source { get; set; }

        public DateTime TimeCreated { get; set; }
    }
}