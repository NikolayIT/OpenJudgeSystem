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

            var users = this.usersData
                .GetAllWithDeleted()
                .Where(u => userIds.Contains(u.Id));

            this.AddUsersToExamGroup(examGroup, users);

            var externalUserIds = userIds.Except(users.Select(u => u.Id)).ToList();

            if (externalUserIds.Any())
            {
                this.backgroundJobs.AddFireAndForgetJob<IExamGroupsBusinessService>(
                    x => x.AddExternalUsersByIdAndUserIds(examGroup.Id, externalUserIds));
            }
        }

        public void AddUsersByIdAndUsernames(int id, IEnumerable<string> usernames)
        {
            var examGroup = this.GetExamGroup(id);

            var users = this.usersData
                .GetAllWithDeleted()
                .Where(u => usernames.Contains(u.UserName));

            this.AddUsersToExamGroup(examGroup, users);

            var externalUsernames = usernames
                .Except(users.Select(u => u.UserName), StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (externalUsernames.Any())
            {
                this.backgroundJobs.AddFireAndForgetJob<IExamGroupsBusinessService>(
                    x => x.AddExternalUsersByIdAndUsernames(examGroup.Id, externalUsernames));
            }
        }

        public void RemoveUsersByIdAndUserIds(int id, IEnumerable<string> userIds)
        {
            var examGroup = this.GetExamGroup(id);

            examGroup.Users = examGroup.Users.Where(u => !userIds.Contains(u.Id)).ToList();

            this.examGroupsData.Update(examGroup);
        }

        public void AddExternalUsersByIdAndUserIds(int id, IEnumerable<string> userIds)
        {
            var examGroup = this.GetExamGroup(id);

            foreach (var userId in userIds)
            {
                this.AddExternalUser(examGroup, userId);
            }
        }

        public void AddExternalUsersByIdAndUsernames(int id, IEnumerable<string> usernames)
        {
            var examGroup = this.GetExamGroup(id);

            foreach (var username in usernames)
            {
                this.AddExternalUser(examGroup, null, username);
            }
        }

        private void AddExternalUser(ExamGroup examGroup, string userId, string username = null)
        {
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

        private void AddUsersToExamGroup(ExamGroup examGroup, IQueryable<UserProfile> users)
        {
            var usersToAdd = users
                .Where(u => u.ExamGroups.All(eg => eg.Id != examGroup.Id))
                .ToList();

            foreach (var user in usersToAdd)
            {
                if (user.IsDeleted)
                {
                    user.IsDeleted = false;
                }

                examGroup.Users.Add(user);
            }

            this.examGroupsData.Update(examGroup);
        }
    }
}