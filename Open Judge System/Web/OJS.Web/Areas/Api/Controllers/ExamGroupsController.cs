namespace OJS.Web.Areas.Api.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data.Models;
    using OJS.Services.Business.Users;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Api.Models;

    public class ExamGroupsController : Controller
    {
        private const string GeneralDateTimeShortFormat = "g";

        private readonly IUsersBusinessService usersBusiness;
        private readonly IExamGroupsDataService examGroupsData;
        private readonly IUsersDataService usersData;

        public ExamGroupsController(
            IUsersBusinessService usersBusiness,
            IExamGroupsDataService examGroupsData,
            IUsersDataService usersData)
        {
            this.usersBusiness = usersBusiness;
            this.examGroupsData = examGroupsData;
            this.usersData = usersData;
        }

        public ActionResult AddUsersToExamGroup(UsersExamGroupModel model)
        {
            if (!this.IsUsersExamGroupModelValid(model))
            {
                return this.Json(false);
            }

            var sulsExamGroup = model.ExamGroupInfoModel;

            var startTime = sulsExamGroup.StartTime?.ToString(GeneralDateTimeShortFormat) ?? string.Empty;
            var endTime = sulsExamGroup.EndTime?.ToString(GeneralDateTimeShortFormat) ?? string.Empty;

            var nameBg = $"{sulsExamGroup.ExamNameBg} - {sulsExamGroup.ExamGroupTrainingLabNameBg} | {startTime} - {endTime}";
            var nameEn = $"{sulsExamGroup.ExamNameEn} - {sulsExamGroup.ExamGroupTrainingLabNameEn} | {startTime} - {endTime}";

            var examGroupExists = true;
            var examGroup = this.examGroupsData.GetByExternalIdAndAppId(sulsExamGroup.Id, model.AppId);

            if (examGroup == null)
            {
                examGroupExists = false;
                examGroup = new ExamGroup
                {
                    ExternalExamGroupId = sulsExamGroup.Id,
                    ExternalAppId = model.AppId
                };
            }

            examGroup.NameBg = nameBg;
            examGroup.NameEn = nameEn;
            
            foreach (var userId in model.UserIds)
            {
                var user = this.usersData.GetByUserIdIncludingDeleted(userId)
                    ?? this.usersBusiness.RegisterById(userId);

                if (user.IsDeleted)
                {
                    user.IsDeleted = false;
                }

                examGroup.Users.Add(user);
            }

            if (examGroupExists)
            {
                this.examGroupsData.Update(examGroup);
            }
            else
            {
                this.examGroupsData.Add(examGroup);
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
            var hasAppId = !string.IsNullOrWhiteSpace(model.AppId);
            var hasRequiredNames = !string.IsNullOrWhiteSpace(model.ExamGroupInfoModel.ExamNameBg) &&
                !string.IsNullOrWhiteSpace(model.ExamGroupInfoModel.ExamNameEn) &&
                !string.IsNullOrWhiteSpace(model.ExamGroupInfoModel.ExamGroupTrainingLabNameBg) &&
                !string.IsNullOrWhiteSpace(model.ExamGroupInfoModel.ExamGroupTrainingLabNameEn);           

            return hasUsers && hasExamGroupId && hasAppId && hasRequiredNames;
        }
    }
}