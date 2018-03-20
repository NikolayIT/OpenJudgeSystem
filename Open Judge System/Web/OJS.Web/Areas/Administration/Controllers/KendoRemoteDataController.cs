namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Contests.Controllers;
    using OJS.Web.Common.Attributes;

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

            var contest = this.contestsData.GetById(contestId);

            var categoryId = contest?.CategoryId;

            var result = new
            {
                contest = contestId,
                category = categoryId
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult GetContestAdminCompetitionActionName(int id)
        {
            var contest = this.contestsData.GetById(id);

            var result = contest?.CanBePracticed == true && !contest.IsActive
                ? CompeteController.PracticeActionName
                : CompeteController.CompeteActionName;

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}