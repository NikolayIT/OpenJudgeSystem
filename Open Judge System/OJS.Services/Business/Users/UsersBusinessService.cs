namespace OJS.Services.Business.Users
{
    using System;

    using OJS.Services.Data.Users;

    public class UsersBusinessService : IUsersBusinessService
    {
        private readonly IUsersDataService usersData;

        public UsersBusinessService(IUsersDataService usersData) =>
            this.usersData = usersData;

        public void AddToExamGroupByIdAndExternalExamGroup(string userId, int? externalExamGroupId)
        {
            //TODO: 
            throw new NotImplementedException();
        }
    }
}