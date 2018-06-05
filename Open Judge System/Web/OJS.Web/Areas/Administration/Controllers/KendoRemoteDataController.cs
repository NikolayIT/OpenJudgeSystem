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
        public JsonResult GetProblemsContaining(string text)
        {
            var problems = this.problemsData.GetAll().Take(DefaultItemsToTake);

            if (!string.IsNullOrWhiteSpace(text))
            {
                problems = this.problemsData
                    .GetAll()
                    .Where(p => p.Name.Contains(text))
                    .Take(DefaultItemsToTake);
            }

            var result = problems
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
        public JsonResult GetContestCompeteOrPracticeActionName(int id)
        {
            var contest = this.contestsData.GetById(id);

            var result = contest?.CanBePracticed == true && !contest.IsActive
                ? CompeteController.PracticeActionName
                : CompeteController.CompeteActionName;

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
            var problems = this.problemsData.GetAllByContest(contestId);

            var result = problems
                .Select(p => new DropdownViewModel
                {
                    Name = p.Name,
                    Id = p.Id
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}