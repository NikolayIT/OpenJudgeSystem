namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.ExamGroups;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.ExamGroups;

    public class ExamGroupsController : LecturerBaseGridController
    {
        private readonly IExamGroupsDataService examGroupsData;

        public ExamGroupsController(
            IOjsData data,
            IExamGroupsDataService examGroupsData)
            : base(data) => this.examGroupsData = examGroupsData;

        public ActionResult Index() => this.View();

        public override IEnumerable GetData() =>
            this.examGroupsData
            .All()
            .Select(ExamGroupAdministrationViewModel.FromExamGroup);

        public override object GetById(object id)
        {
            throw new System.NotImplementedException();
        }

        public ActionResult Create()
        {
            return this.View();
        }
    }
}