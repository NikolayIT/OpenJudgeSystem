namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Controllers;

    using ModelType = OJS.Web.Areas.Administration.ViewModels.News.NewsAdministrationViewModel;

    public class NewsController : KendoGridAdministrationController
    {
        public NewsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.News.All()
                .Where(news => !news.IsDeleted)
                .Select(ModelType.ViewModel);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseCreate(request, model.ToEntity);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseUpdate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseDestroy(request, model.ToEntity);
        }
    }
}