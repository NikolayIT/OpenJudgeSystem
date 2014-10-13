namespace OJS.Web.Areas.Administration.Controllers.Common
{
    using System.Linq;

    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    [AuthorizeRoles(SystemRole.Administrator, SystemRole.Lecturer)]
    public abstract class LecturerBaseController : AdministrationController
    {
        protected LecturerBaseController(IOjsData data)
            : base(data)
        {
        }

        protected bool CheckIfUserHasContestPermissions(int contestId)
        {
            var contest = this.Data.Contests.GetById(contestId);

            if (contest == null)
            {
                return false;
            }

            return contest.Lecturers.Any(x => x.LecturerId == this.UserProfile.Id) || this.User.IsAdmin();
        }

        protected bool CheckIfUserHasProblemPermissions(int problemId)
        {
            var problem = this.Data.Problems.GetById(problemId);

            if (problem == null)
            {
                return false;
            }

            return problem.Contest.Lecturers.Any(x => x.LecturerId == this.UserProfile.Id) || this.User.IsAdmin();
        }
    }
}