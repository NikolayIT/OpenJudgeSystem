namespace OJS.Services.Business.ExamGroups
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;

    using OJS.Common;
    using OJS.Services.Common.HttpRequester;
    using OJS.Services.Common.HttpRequester.Models.Users;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.Users;

    public class ExamGroupsBusinessService : IExamGroupsBusinessService
    {
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

        public async Task AddUsersByIdAndUserIds(int id, IEnumerable<string> userIds)
        {
            var examGroup = this.examGroupsData.GetById(id);

            if (examGroup == null)
            {
                throw new ArgumentNullException(nameof(examGroup), "Exam group cannot be null");
            }

            foreach (var userId in userIds)
            {
                var user = await this.usersData.GetByIdIncludingDeletedAsync(userId);

                if (user == null)
                {
                    ExternalUserViewModel externalUser;

                    // TODO: inject strings
                    var response = await this.httpRequester.GetAsync<ExternalUserViewModel>(
                        userId,
                        string.Format(UrlConstants.GetUserInfoById, "https://localhost:44308"),
                        "apiKey");

                    if (response.IsSuccess)
                    {
                        externalUser = response.Data;
                    }
                    else
                    {
                        throw new HttpException(response.ErrorMessage);
                    }

                    if (externalUser != null)
                    {
                        user = externalUser.Entity;
                    }
                }
                else
                {
                    if (user.IsDeleted)
                    {
                        user.IsDeleted = false;
                    }
                }

                examGroup.Users.Add(user);

                this.examGroupsData.Update(examGroup);
            }
        }
    }
}