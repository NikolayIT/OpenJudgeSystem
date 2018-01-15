namespace OJS.Web.Areas.Api.Controllers
{
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Data.Models;
    using OJS.Services.Business.ExamGroups;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.ExamGroups;
    using OJS.Web.Areas.Api.Models;
    using OJS.Web.ViewModels.Account;

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

            if (examGroupExists)
            {
                this.examGroupsData.Update(examGroup);
            }
            else
            {
                this.examGroupsData.Add(examGroup);
            }

            var examGroupId = this.examGroupsData.GetByExternalIdAndAppId(sulsExamGroup.Id, model.AppId).Id;

            this.backgroundJobs.AddFireAndForgetJob<IExamGroupsBusinessService>(
                x => x.AddUsersByIdAndUserIds(examGroupId, model.UserIds));

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

        private async Task<ExternalUserViewModel> GetExternalUserInfoByIdAsync(string userId)
        {
            using (var httpClient = new HttpClient())
            {
                var jsonMediaType = new MediaTypeWithQualityHeaderValue(GlobalConstants.JsonMimeType);
                httpClient.DefaultRequestHeaders.Accept.Add(jsonMediaType);

                var response = await httpClient.PostAsJsonAsync(Settings.GetExternalUserUrl, new { userId });
                if (response.IsSuccessStatusCode)
                {
                    var externalUser = await response.Content.ReadAsAsync<ExternalUserViewModel>();
                    return externalUser;
                }

                throw new HttpException((int)response.StatusCode, "An error has occurred while connecting to the external system.");
            }
        }
    }
}