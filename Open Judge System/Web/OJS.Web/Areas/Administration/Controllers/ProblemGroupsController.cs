namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Web.Areas.Administration.Controllers.Common;

    using DetailViewModelType = OJS.Web.Areas.Administration.ViewModels.Problem.ProblemViewModel;
    using Resource = Resources.Areas.Administration.ProblemGroups.ProblemGroupsController;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ProblemGroup.DetailedProblemGroupViewModel;

    public class ProblemGroupsController : LecturerBaseGridController
    {
        private readonly IProblemGroupsDataService problemGroupsData;

        public ProblemGroupsController(
            IOjsData data,
            IProblemGroupsDataService problemGroupsData)
            : base(data)
        {
            this.problemGroupsData = problemGroupsData;
        }

        public override IEnumerable GetData() =>
            this.problemGroupsData
                .GetAll()
                .Select(ViewModelType.FromProblemGroup);

        public override object GetById(object id) => this.GetByIdAsNoTracking((int)id);

        public ActionResult Index() => this.View();

        [HttpPost]
        public ActionResult Create(
            [DataSourceRequest] DataSourceRequest request,
            ViewModelType model)
        {
            this.BaseCreate(model.GetEntityModel());
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, ViewModelType model)
        {
            var problemGroup = model.GetEntityModel(this.GetByIdAsNoTracking(model.Id));

            this.BaseUpdate(problemGroup);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public JsonResult ProblemsInProblemGroup([DataSourceRequest]DataSourceRequest request, int id)
        {
            var problems = this.problemGroupsData
                .GetProblemsById(id)
                .Select(DetailViewModelType.FromProblem);

            return this.Json(problems.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private ProblemGroup GetByIdAsNoTracking(int id) =>
            this.problemGroupsData.GetByIdQuery(id).AsNoTracking().SingleOrDefault();
    }
}