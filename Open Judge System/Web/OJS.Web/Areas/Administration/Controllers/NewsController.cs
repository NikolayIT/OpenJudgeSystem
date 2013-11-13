namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Controllers;

    using ModelType = OJS.Web.Areas.Administration.ViewModels.NewsViewModel;

    public class NewsController : KendoGridAdministrationController
    {
        public NewsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.News.All().Where(x => !x.IsDeleted).Select(ModelType.ViewModel);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseCreate(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseUpdate(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseDestroy(request, model);
        }
    }
}