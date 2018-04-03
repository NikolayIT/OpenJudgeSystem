namespace OJS.Services.Business.ExamGroups
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using OJS.Common;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Common.HttpRequester;
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
            var examGroup = this.examGroupsData.GetById(id);

            if (examGroup == null)
            {
                throw new ArgumentNullException(nameof(examGroup), ExamGroupCannotBeNullMessage);
            }

            foreach (var userId in userIds)
            {
                var user = this.usersData.GetByIdIncludingDeleted(userId);

                if (user != null)
                {
                    if (user.IsDeleted)
                    {
                        user.IsDeleted = false;
                    }

                    examGroup.Users.Add(user);
                }
                else
                {
                    this.backgroundJobs.AddFireAndForgetJob<IExamGroupsBusinessService>(
                        x => x.AddExternalUserByIdAndUser(examGroup.Id, userId));
                }
            }

            this.examGroupsData.Update(examGroup);
        }

        public void RemoveUsersByIdAndUserIds(int id, IEnumerable<string> userIds)
        {
            var examGroup = this.examGroupsData.GetById(id);

            if (examGroup == null)
            {
                throw new ArgumentNullException(nameof(examGroup), ExamGroupCannotBeNullMessage);
            }

            examGroup.Users = examGroup.Users.Where(u => !userIds.Contains(u.Id)).ToList();

            this.examGroupsData.Update(examGroup);
        }

        public void AddExternalUserByIdAndUser(int id, string userId)
        {
            var examGroup = this.examGroupsData.GetById(id);

            if (examGroup == null)
            {
                throw new ArgumentNullException(nameof(examGroup), ExamGroupCannotBeNullMessage);
            }

            var response = this.httpRequester.Get<ExternalUserInfoModel>(
                new { userId },
                string.Format(UrlConstants.GetUserInfoByIdApiFormat, this.sulsPlatformBaseUrl),
                this.apiKey);

            if (response.IsSuccess && response.Data != null)
            {
                var user = response.Data.Entity;
                examGroup.Users.Add(user);
                this.examGroupsData.Update(examGroup);
            }
            else
            {
                throw new HttpException(response.ErrorMessage);
            }
        }
    }
}