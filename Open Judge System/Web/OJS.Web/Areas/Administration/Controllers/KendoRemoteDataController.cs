namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Common.Extensions;
    using OJS.Web.ViewModels.Common;

    using GeneralResource = Resources.Areas.Administration.AdministrationGeneral;

    public class KendoRemoteDataController : KendoRemoteDataBaseController
    {
        private readonly IUsersDataService usersData;
        private readonly IContestsDataService contestsData;

        public KendoRemoteDataController(
            IOjsData data,
            IUsersDataService usersData,
            IContestsDataService contestsData)
            : base(data)
        {
            this.usersData = usersData;
            this.contestsData = contestsData;
        }

        [AjaxOnly]
        public JsonResult GetUsersContaining(string userFilter)
        {
            var users = this.usersData.GetAll().Take(DefaultItemsToTake);

            if (!string.IsNullOrWhiteSpace(userFilter))
            {
                users = this.usersData
                    .GetAll()
                    .Where(u => u.UserName.ToLower().Contains(userFilter.ToLower()))
                    .Take(DefaultItemsToTake);
            }

            var result = users
                .Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.Id
                })
                .ToList();

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult GetContestInformation(int id)
        {
            var contestId = id;

            if (!this.CheckIfUserHasContestPermissions(contestId))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.Json("No permissions");
            }

            var contest = this.contestsData.GetById(contestId);

            var categoryId = contest?.CategoryId;

            var result = new
            {
                contest = contestId,
                category = categoryId
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}