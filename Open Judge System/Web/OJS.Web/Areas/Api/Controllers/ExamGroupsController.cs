namespace OJS.Web.Areas.Api.Controllers
{
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data.Models;
    using OJS.Services.Business.ExamGroups;
    using OJS.Services.Business.Users;
    using OJS.Web.Areas.Api.Models;

    public class ExamGroupsController : Controller
    {
        private readonly IUsersBusinessService usersBusiness;
        private readonly IExamGroupsBusinessService examGroupsBusiness;

        public ExamGroupsController(
            IUsersBusinessService usersBusiness,
            IExamGroupsBusinessService examGroupsBusiness)
        {
            this.usersBusiness = usersBusiness;
            this.examGroupsBusiness = examGroupsBusiness;
        }

        public ActionResult AddUsersToExamGroup(UsersExamGroupModel model)
        {
            if (!this.IsUsersExamGroupModelValid(model))
            {
                return this.Json(false);
            }

            var sulsExamGroup = model.ExamGroupInfoModel;
            var startTimeBg = sulsExamGroup.StartTime.Value.ToString("f", CultureInfo.CreateSpecificCulture("bg-BG"));
            var startTimeEn = sulsExamGroup.StartTime.Value.ToString("f", CultureInfo.CreateSpecificCulture("en-EN"));

            var examGroup = new ExamGroup
            {
                ExternalExamGroupId = sulsExamGroup.Id,
                AppTenant = model.AppTenant,
                NameBg = $"{sulsExamGroup.ExamNameBg}||{startTimeBg}||{sulsExamGroup.ExamGroupTrainingLabNameBg}",
                NameEn = $"{sulsExamGroup.ExamNameEn}||{startTimeEn}||{sulsExamGroup.ExamGroupTrainingLabNameEn}"
            };

            this.examGroupsBusiness.AddOrUpdate(examGroup);

            foreach (var userId in model.UserIds)
            {
                this.usersBusiness.AddToExamGroupByIdAndExternalExamGroup(userId, examGroup.ExternalExamGroupId);
            }

            return this.Json(true);
        }

        public ActionResult RemoveUsersFromExamGroup(UsersExamGroupModel model)
        {
            if (!this.IsUsersExamGroupModelValid(model))
            {
                return this.Json(false);
            }

            // TODO: add logc for removing users from exam group
            return this.Json(true);
        }

        private bool IsUsersExamGroupModelValid(UsersExamGroupModel model)
        {
            var hasUsers = model.UserIds.Any();
            var hasExamGroupId = model.ExamGroupInfoModel.Id != default(int);
            var hasAppTenant = !string.IsNullOrWhiteSpace(model.AppTenant);
            var hasStartTime = model.ExamGroupInfoModel.StartTime.HasValue;
            var hasRequiredNames = !string.IsNullOrWhiteSpace(model.ExamGroupInfoModel.ExamNameBg) &&
                !string.IsNullOrWhiteSpace(model.ExamGroupInfoModel.ExamNameEn) &&
                !string.IsNullOrWhiteSpace(model.ExamGroupInfoModel.ExamGroupTrainingLabNameBg) &&
                !string.IsNullOrWhiteSpace(model.ExamGroupInfoModel.ExamGroupTrainingLabNameEn);           

            return hasUsers && hasExamGroupId && hasAppTenant && hasRequiredNames && hasStartTime;
        }
    }
}