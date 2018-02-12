namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Expressions;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;
    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Business.ProblemGroups;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Common.Extensions;

    using DetailViewModelType = OJS.Web.Areas.Administration.ViewModels.Problem.ProblemViewModel;
    using GeneralResource = Resources.Areas.Administration.AdministrationGeneral;
    using Resource = Resources.Areas.Administration.ProblemGroups.ProblemGroupsController;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ProblemGroup.DetailedProblemGroupViewModel;

    public class ProblemGroupsController : LecturerBaseGridController
    {
        private readonly IProblemGroupsDataService problemGroupsData;
        private readonly IProblemGroupsBusinessService problemGroupsBusiness;
        private readonly IContestsDataService contestsData;

        public ProblemGroupsController(
            IOjsData data,
            IProblemGroupsDataService problemGroupsData,
            IProblemGroupsBusinessService problemGroupsBusiness,
            IContestsDataService contestsData)
            : base(data)
        {
            this.problemGroupsData = problemGroupsData;
            this.problemGroupsBusiness = problemGroupsBusiness;
            this.contestsData = contestsData;
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
            if (!this.IsModelAndContestValid(model))
            {
                return this.GridOperation(request, model);
            }

            this.BaseCreate(model.GetEntityModel());
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, ViewModelType model)
        {
            if (!this.IsModelAndContestValid(model))
            {
                return this.GridOperation(request, model);
            }

            var problemGroup = model.GetEntityModel(this.GetByIdAsNoTracking(model.Id));

            this.BaseUpdate(problemGroup);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest] DataSourceRequest request, ViewModelType model)
        {
            if (!this.IsModelAndContestValid(model))
            {
                return this.GridOperation(request, model);
            }

            var result = this.problemGroupsBusiness.DeleteById(model.Id);

            if (result.IsError)
            {
                this.ModelState.AddModelError(string.Empty, result.Error);
            }

            return this.GridOperation(request, model);
        }

        public ActionResult Contest(int? id)
        {
            if (id != null && !this.CheckIfUserHasContestPermissions(id.Value))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.RedirectToAction<ContestsController>(c => c.Index(), new { area = "Administration" });
            }

            this.ViewBag.ContestId = id;

            return this.View(GlobalConstants.Index);
        }

        [HttpPost]
        public JsonResult ProblemsInProblemGroup([DataSourceRequest]DataSourceRequest request, int id)
        {
            var problems = this.problemGroupsData
                .GetProblemsById(id)
                .Select(DetailViewModelType.FromProblem);

            return this.Json(problems.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private bool IsModelAndContestValid(ViewModelType model)
        {
            if (string.IsNullOrWhiteSpace(model.ContestName))
            {
                this.ModelState.AddModelError(nameof(model.ContestName), Resource.Contest_required);
            }

            if (!this.contestsData.ExistsById(model.ContestId))
            {
                this.ModelState.AddModelError(nameof(model.ContestName), Resource.Contest_does_not_exist);
            }

            if (!this.CheckIfUserHasContestPermissions(model.ContestId))
            {
                this.ModelState.AddModelError(string.Empty, GeneralResource.No_privileges_message);
            }

            return this.ModelState.IsValid;
        }

        private ProblemGroup GetByIdAsNoTracking(int id) =>
            this.problemGroupsData.GetByIdQuery(id).AsNoTracking().SingleOrDefault();
    }
}