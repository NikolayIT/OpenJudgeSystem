namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Infrastructure.Filters.Attributes;

    using DatabaseModelType = OJS.Data.Models.ContestCategory;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ContestCategory.ContestCategoryAdministrationViewModel;

    public class ContestCategoriesController : AdministrationBaseGridController
    {
        public ContestCategoriesController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.ContestCategories
                .All()
                .Where(cat => !cat.IsDeleted)
                .Select(ViewModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.ContestCategories
                .All()
                .FirstOrDefault(o => o.Id == (int)id);
        }

        public override string GetEntityKeyName() =>
            this.GetEntityKeyNameByType(typeof(DatabaseModelType));

        public ActionResult Index() => this.View();

        [HttpPost]
        [ClearMainContestCategoriesCache]
        [ClearContestCategoryCache(queryKeyForCategoryId: nameof(ViewModelType.Id))]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var databaseModel = model.GetEntityModel();
            model.Id = (int)this.BaseCreate(databaseModel);
            this.UpdateAuditInfoValues(model, databaseModel);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        [ClearMainContestCategoriesCache]
        [ClearContestCategoryCache(queryKeyForCategoryId: nameof(ViewModelType.Id))]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var entity = this.GetById(model.Id) as DatabaseModelType;
            this.BaseUpdate(model.GetEntityModel(entity));
            this.UpdateAuditInfoValues(model, entity);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        [ClearMainContestCategoriesCache]
        [ClearContestCategoryCache(queryKeyForCategoryId: nameof(ViewModelType.Id))]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var contest = this.Data.ContestCategories.GetById(model.Id.Value);
            this.CascadeDeleteCategories(contest);
            return this.Json(this.ModelState.ToDataSourceResult());
        }

        public ActionResult Hierarchy() => this.View();

        public ActionResult ReadCategories(int? id)
        {
            var categories =
                this.Data.ContestCategories.All()
                    .Where(x => x.IsVisible)
                    .Where(x => id.HasValue ? x.ParentId == id : x.ParentId == null)
                    .OrderBy(x => x.OrderBy)
                    .Select(x => new { id = x.Id, hasChildren = x.Children.Any(), x.Name, });

            return this.Json(categories, JsonRequestBehavior.AllowGet);
        }

        [ClearMainContestCategoriesCache]
        [ClearMovedContestCategoryCache("id", "to")]
        public void MoveCategory(int id, int? to)
        {
            if (id != to)
            {
                var category = this.Data.ContestCategories.GetById(id);
                category.ParentId = to;
                this.Data.SaveChanges();
            }
        }

        private void CascadeDeleteCategories(DatabaseModelType contest)
        {
            foreach (var children in contest.Children.ToList())
            {
                this.CascadeDeleteCategories(children);
            }

            this.Data.ContestCategories.Delete(contest);
            this.Data.SaveChanges();
        }
    }
}