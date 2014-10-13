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
            return this.User.IsAdmin() ||
                   this.Data.Contests
                       .All()
                       .Any(x => x.Id == contestId && x.Lecturers.Any(y => y.LecturerId == this.UserProfile.Id));
        }

        protected bool CheckIfUserHasProblemPermissions(int problemId)
        {
            return this.User.IsAdmin() ||
                   this.Data.Problems
                       .All()
                       .Any(x => x.Id == problemId && x.Contest.Lecturers.Any(y => y.Lecturer.Id == this.UserProfile.Id));
        }
    }
}