namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Data;
    using OJS.Web.Areas.Administration.Providers;
    using OJS.Web.Areas.Administration.Providers.Contracts;
    using OJS.Web.Areas.Administration.Controllers.Common;

    using Resources.News.Views;

    using DatabaseModelType = OJS.Data.Models.News;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.News.NewsAdministrationViewModel;

    public class NewsController : AdministrationBaseGridController
    {
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
            var databaseModel = model.GetEntityModel();
            model.Id = (int)this.BaseCreate(databaseModel);
            this.UpdateAuditInfoValues(model, databaseModel);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var entity = this.GetById(model.Id) as DatabaseModelType;
            this.BaseUpdate(model.GetEntityModel(entity));
            this.UpdateAuditInfoValues(model, entity);
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
            var providers = new List<INewsProvider>
            {
                //// new InfoManNewsProvider(),
                //// new InfosNewsProvider(),
                new SoftUniNewsProvier()
            };

            var allNews = new List<DatabaseModelType>();

            foreach (var newsProvider in providers)
            {
                allNews.AddRange(newsProvider.FetchNews());
            }

            this.PopulateDatabaseWithNews(allNews);

            this.TempData[GlobalConstants.InfoMessage] = All.News_successfully_added;
            return this.RedirectToAction("All", "News", new { Area = string.Empty });
        }

        private void PopulateDatabaseWithNews(IEnumerable<DatabaseModelType> fetchedNews)
        {
            foreach (var news in fetchedNews)
            {
                if (!string.IsNullOrEmpty(news.Title) &&
                    !string.IsNullOrEmpty(news.Content) &&
                    news.Content.Length > 10 &&
                    !this.Data.News.All().Any(existingNews => existingNews.Title == news.Title))
                {
                    this.Data.News.Add(news);
                }

                this.Data.SaveChanges();
            }
        }
    }
}