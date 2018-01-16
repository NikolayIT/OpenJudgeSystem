namespace OJS.Web.Areas.Api.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data.Models;
    using OJS.Services.Business.ExamGroups;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.ExamGroups;
    using OJS.Web.Areas.Api.Models;

    public class ExamGroupsController : Controller
    {
        private const string GeneralDateTimeShortFormat = "g";

        private readonly IExamGroupsDataService examGroupsData;
        private readonly IHangfireBackgroundJobService backgroundJobs;

        public ExamGroupsController(
            IExamGroupsDataService examGroupsData,
            IHangfireBackgroundJobService backgroundJobs)
        {
            this.examGroupsData = examGroupsData;
            this.backgroundJobs = backgroundJobs;
        }

        public ActionResult AddUsersToExamGroup(UsersExamGroupModel model)
        {
            if (!this.IsUsersExamGroupModelValid(model))
            {
                return this.Json(false);
            }

            var externalExamGroup = model.ExamGroupInfoModel;

            var startTime = externalExamGroup.StartTime?.ToString(GeneralDateTimeShortFormat) ?? string.Empty;
            var endTime = externalExamGroup.EndTime?.ToString(GeneralDateTimeShortFormat) ?? string.Empty;

            var nameBg = $"{externalExamGroup.ExamNameBg} - {externalExamGroup.ExamGroupTrainingLabNameBg} | {startTime} - {endTime}";
            var nameEn = $"{externalExamGroup.ExamNameEn} - {externalExamGroup.ExamGroupTrainingLabNameEn} | {startTime} - {endTime}";

            var examGroupExists = true;
            var examGroup = this.examGroupsData.GetByExternalIdAndAppId(externalExamGroup.Id, model.AppId);

            if (examGroup == null)
            {
                examGroupExists = false;
                examGroup = new ExamGroup
                {
                    ExternalExamGroupId = externalExamGroup.Id,
                    ExternalAppId = model.AppId
                };
            }

            examGroup.NameBg = nameBg;
            examGroup.NameEn = nameEn;

            if (examGroupExists)
            {
                this.examGroupsData.Update(examGroup);
            }
            else
            {
                this.examGroupsData.Add(examGroup);
            }

            var examGroupId = this.examGroupsData
                .GetByExternalIdAndAppId(externalExamGroup.Id, model.AppId)?.Id;

            if (examGroupId.HasValue)
            {
                this.backgroundJobs.AddFireAndForgetJob<IExamGroupsBusinessService>(
                    x => x.AddUsersByIdAndUserIds(examGroupId.Value, model.UserIds));
            }

            return this.Json(true);
        }

        public ActionResult RemoveUsersFromExamGroup(UsersExamGroupModel model)
        {
            if (!this.IsUsersExamGroupModelValid(model))
            {
                return this.Json(false);
            }

            var examGroupId = this.examGroupsData
                .GetByExternalIdAndAppId(model.ExamGroupInfoModel.Id, model.AppId)?.Id;

            if (examGroupId.HasValue)
            {
                this.backgroundJobs.AddFireAndForgetJob<IExamGroupsBusinessService>(
                    x => x.RemoveUsersByIdAndUserIds(examGroupId.Value, model.UserIds));
            }

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