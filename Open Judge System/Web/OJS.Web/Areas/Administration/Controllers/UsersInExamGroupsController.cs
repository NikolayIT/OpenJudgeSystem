namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Services.Data.ExamGroups;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.ExamGroups;

    public class UsersInExamGroupsController : LecturerBaseGridController
    {
        private readonly IExamGroupsDataService examGroupsData;
        private int examGroupId;

        public UsersInExamGroupsController(
            IOjsData data,
            IExamGroupsDataService examGroupsData)
                : base(data) => this.examGroupsData = examGroupsData;

        [HttpPost]
        public JsonResult UsersInExamGroup([DataSourceRequest]DataSourceRequest request, int id)
        {
            this.examGroupId = id;
            var users = this.GetData();
      
            return this.Json(users.ToDataSourceResult(request));
        }

        public override IEnumerable GetData() =>
            this.examGroupsData
            .GetUsersById(this.examGroupId)
            .Select(UserInExamGroupViewModel.FromUserProfile);

        public override object GetById(object id)
        {
            throw new System.NotImplementedException();
        }
    }
}