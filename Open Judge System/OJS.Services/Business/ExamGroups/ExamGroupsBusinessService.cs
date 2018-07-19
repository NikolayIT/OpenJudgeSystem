namespace OJS.Services.Business.ExamGroups
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using OJS.Common;
    using OJS.Data.Models;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Common.HttpRequester;
    using OJS.Services.Common.HttpRequester.Models;
    using OJS.Services.Common.HttpRequester.Models.Users;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.Users;

    public class ExamGroupsBusinessService : IExamGroupsBusinessService
    {
        // TODO: Add to Resource
        private const string ExamGroupCannotBeNullMessage = "Exam group cannot be null";

        private readonly IExamGroupsDataService examGroupsData;
        private readonly IUsersDataService usersData;
        private readonly IHttpRequesterService httpRequester;
        private readonly IHangfireBackgroundJobService backgroundJobs;
        private readonly string sulsPlatformBaseUrl;
        private readonly string apiKey;

        public ExamGroupsBusinessService(
            IExamGroupsDataService examGroupsData,
            IUsersDataService usersData,
            IHttpRequesterService httpRequester,
            IHangfireBackgroundJobService backgroundJobs,
            string sulsPlatformBaseUrl,
            string apiKey)
        {
            this.examGroupsData = examGroupsData;
            this.usersData = usersData;
            this.httpRequester = httpRequester;
            this.backgroundJobs = backgroundJobs;
            this.sulsPlatformBaseUrl = sulsPlatformBaseUrl;
            this.apiKey = apiKey;
        }

        public void AddUsersByIdAndUserIds(int id, IEnumerable<string> userIds)
        {
            var examGroup = this.GetExamGroup(id);

            foreach (var userId in userIds)
            {
                var user = this.usersData.GetByIdIncludingDeleted(userId);

                if (user != null)
                {
                    AddUserToExamGroup(examGroup, user);
                }
                else
                {
                    this.backgroundJobs.AddFireAndForgetJob<IExamGroupsBusinessService>(
                        x => x.AddExternalUserByIdAndUser(examGroup.Id, userId));
                }
            }

            this.examGroupsData.Update(examGroup);
        }

        public void AddUsersByIdAndUsernames(int id, IEnumerable<string> usernames)
        {
            var examGroup = this.GetExamGroup(id);

            foreach (var username in usernames)
            {
                var user = this.usersData.GetByUsernameIncludingDeleted(username);

                if (user != null)
                {
                    AddUserToExamGroup(examGroup, user);
                }
                else
                {
                    this.backgroundJobs.AddFireAndForgetJob<IExamGroupsBusinessService>(
                        x => x.AddExternalUserByIdAndUsername(examGroup.Id, username));
                }
            }

            this.examGroupsData.Update(examGroup);
        }

        public void RemoveUsersByIdAndUserIds(int id, IEnumerable<string> userIds)
        {
            var examGroup = this.GetExamGroup(id);

            examGroup.Users = examGroup.Users.Where(u => !userIds.Contains(u.Id)).ToList();

            this.examGroupsData.Update(examGroup);
        }

        public void AddExternalUserByIdAndUser(int id, string userId) =>
            this.AddExternalUser(id, userId);

        public void AddExternalUserByIdAndUsername(int id, string username) =>
            this.AddExternalUser(id, null, username);

        private void AddExternalUser(int id, string userId, string username = null)
        {
            var examGroup = this.GetExamGroup(id);

            ExternalDataRetrievalResult<ExternalUserInfoModel> response;

            if (userId != null)
            {
                response = this.httpRequester.Get<ExternalUserInfoModel>(
                    new { userId },
                    string.Format(UrlConstants.GetUserInfoByIdApiFormat, this.sulsPlatformBaseUrl),
                    this.apiKey);
            }
            else if (username != null)
            {
                response = this.httpRequester.Get<ExternalUserInfoModel>(
                    new { username },
                    string.Format(UrlConstants.GetUserInfoByUsernameApiFormat, this.sulsPlatformBaseUrl),
                    this.apiKey);
            }
            else
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (response.IsSuccess)
            {
                if (response.Data == null)
                {
                    return;
                }

                var user = response.Data.Entity;
                examGroup.Users.Add(user);
                this.examGroupsData.Update(examGroup);
            }
            else
            {
                throw new HttpException(response.ErrorMessage);
            }
        }

        private ExamGroup GetExamGroup(int examGroupId)
        {
            var examGroup = this.examGroupsData.GetById(examGroupId);

            if (examGroup == null)
            {
                throw new ArgumentNullException(nameof(examGroup), ExamGroupCannotBeNullMessage);
            }

            return examGroup;
        }

        private static void AddUserToExamGroup(ExamGroup examGroup, UserProfile user)
        {
            if (user.IsDeleted)
            {
                user.IsDeleted = false;
            }

            examGroup.Users.Add(user);
        }
    }
}