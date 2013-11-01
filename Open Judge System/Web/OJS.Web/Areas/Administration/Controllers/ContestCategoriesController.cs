namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Controllers;

    using ModelType = OJS.Data.Models.ContestCategory;

    public class ContestCategoriesController : KendoGridAdministrationController
    {
        public ContestCategoriesController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.ContestCategories.All().Where(x => !x.IsDeleted);
        }

        public ActionResult Index()
        {
            return this.View();
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

        public ActionResult Hierarchy()
        {
            return this.View();
        }

        public ActionResult ReadCategories(int? id)
        {
            var categories =
                this.Data.ContestCategories.All()
                    .Where(x => x.IsVisible)
                    .Where(x => id.HasValue ? x.ParentId == id : x.ParentId == null)
                    .OrderBy(x => x.OrderBy)
                    .Select(x => new { id = x.Id, hasChildren = x.Children.Any(), Name = x.Name, });

            return this.Json(categories, JsonRequestBehavior.AllowGet);
        }

        public void MoveCategory(int id, int? to)
        {
            var category = this.Data.ContestCategories.GetById(id);
            category.ParentId = to;
            this.Data.SaveChanges();
        }
    }
}