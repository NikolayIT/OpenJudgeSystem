namespace OJS.Web.Areas.Api.Controllers
{
    using System.Web.Mvc;

    using OJS.Services.Data.Users;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Infrastructure.Filters.Attributes;

    [ValidateRemoteDataApiKey]
    public class UserRelatedEventsReceiverController : ApiController
    {
        private readonly IUsersDataService usersData;

        public UserRelatedEventsReceiverController(IUsersDataService usersData) =>
            this.usersData = usersData;

        [HttpPost]
        public ActionResult UserConfirmedForDeletion(string apiKey, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return this.JsonError($"{nameof(userId)} is null or white space");
            }

            this.usersData.DeleteById(userId);

            return this.JsonSuccess(true);
        }
    }
}