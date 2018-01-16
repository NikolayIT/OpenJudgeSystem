namespace OJS.Services.Business.ExamGroups
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using OJS.Common;
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

        public ExamGroupsBusinessService(
            IExamGroupsDataService examGroupsData,
            IUsersDataService usersData,
            IHttpRequesterService httpRequester)
        {
            this.examGroupsData = examGroupsData;
            this.usersData = usersData;
            this.httpRequester = httpRequester;
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

                if (user == null)
                {
                    // TODO: inject strings
                    var response = this.httpRequester.Get<ExternalUserViewModel>(
                        new { userId },
                        string.Format(UrlConstants.GetUserInfoById, "https://localhost:44308"),
                        "apiKey");

                    if (response.IsSuccess && response.Data != null)
                    {
                        user = response.Data.Entity;
                    }
                    else
                    {
                        throw new HttpException(response.ErrorMessage);
                    }
                }

                if (user.IsDeleted)
                {
                    user.IsDeleted = false;
                }

                examGroup.Users.Add(user);
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
    }
}