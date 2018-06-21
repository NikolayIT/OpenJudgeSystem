namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Services.Data.Problems;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Contests.Controllers;
    using OJS.Web.Common.Attributes;
    using OJS.Web.ViewModels.Common;

    public class KendoRemoteDataController : KendoRemoteDataBaseController
    {
        private readonly IUsersDataService usersData;
        private readonly IContestsDataService contestsData;
        private readonly IProblemsDataService problemsData;
        private readonly IProblemGroupsDataService problemGroupsData;

        public KendoRemoteDataController(
            IOjsData data,
            IUsersDataService usersData,
            IContestsDataService contestsData,
            IProblemsDataService problemsData,
            IProblemGroupsDataService problemGroupsData)
            : base(data)
        {
            this.usersData = usersData;
            this.contestsData = contestsData;
            this.problemsData = problemsData;
            this.problemGroupsData = problemGroupsData;
        }

        [AjaxOnly]
        public JsonResult GetUsersContaining(string userFilter)
        {
            var users = this.usersData.GetAll();

            if (!string.IsNullOrWhiteSpace(userFilter))
            {
                users = users.Where(u => u.UserName.ToLower().Contains(userFilter.ToLower()));
            }

            var result = users
                .Take(DefaultItemsToTake)
                .Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.Id
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult GetProblemsContaining(string text)
        {
            var problems = this.problemsData.GetAll();

            if (!string.IsNullOrWhiteSpace(text))
            {
                problems = problems.Where(p => p.Name.Contains(text));
            }

            var result = problems
                .Take(DefaultItemsToTake)
                .Select(p => new DropdownViewModel
                {
                    Id = p.Id,
                    Name = p.Name
                });

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
        public JsonResult GetContestNameAndCompeteOrPracticeActionName(int id)
        {
            var contest = this.contestsData.GetById(id);

            var actionName = contest?.CanBePracticed == true && !contest.IsActive
                ? CompeteController.PracticeActionName
                : CompeteController.CompeteActionName;

            var contestName = contest?.Name ?? string.Empty;

            var result = new
            {
                actionName,
                contestName
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult GetCascadeProblemGroupsFromContest(int contestId)
        {
            var result = this.problemGroupsData
                .GetAllByContest(contestId)
                .OrderBy(pg => pg.OrderBy)
                .Select(pg => new DropdownViewModel
                {
                    Name = pg.OrderBy.ToString(),
                    Id = pg.Id
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult GetCascadeProblemsFromContest(int contestId)
        {
            var result = this.problemsData
                .GetAllByContest(contestId)
                .Select(p => new DropdownViewModel
                {
                    Name = p.Name,
                    Id = p.Id
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}