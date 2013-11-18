namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Controllers;

    using ModelType = OJS.Web.Areas.Administration.ViewModels.Submission.SubmissionAdministrationViewModel;

    public class SubmissionsController : KendoGridAdministrationController
    {
        public SubmissionsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Submissions
                .All()
                .Select(ModelType.ViewModel);
        }

        public ActionResult Index()
        {
            this.GenerateProblemsDropDownData();
            this.GenerateParticipantsDropDownData();
            this.GenerateSubmissionTypesDropDownData();
            return this.View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseCreate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseUpdate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseDestroy(request, model.ToEntity);
        }

        private void GenerateParticipantsDropDownData()
        {
            var dropDownData = this.Data.Users
                .All()
                .ToList()
                .Select(user => new SelectListItem
                {
                    Text = user.UserName,
                    Value = user.Id,
                });

            // TODO: Improve not to use ViewData
            this.ViewData["ParticipantIdData"] = dropDownData;
        }

        private void GenerateSubmissionTypesDropDownData()
        {
            var dropDownData = this.Data.SubmissionTypes
                .All()
                .ToList()
                .Select(subm => new SelectListItem
                {
                    Text = subm.Name,
                    Value = subm.Id.ToString(),
                });

            // TODO: Improve not to use ViewData
            this.ViewData["SubmissionTypeIdData"] = dropDownData;
        }

        private void GenerateProblemsDropDownData()
        {
            var dropDownData = this.Data.Problems
                .All()
                .ToList()
                .Select(pr => new SelectListItem
                {
                    Text = pr.Name,
                    Value = pr.Id.ToString(),
                });

            // TODO: Improve not to use ViewData
            this.ViewData["ProblemIdData"] = dropDownData;
        }
    }
}