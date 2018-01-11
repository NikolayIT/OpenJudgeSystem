namespace OJS.Web.Areas.Api.Controllers
{
    using System.Web.Mvc;

    using OJS.Web.Areas.Api.Models;

    public class ExamGroupsController : Controller
    {
        public ActionResult AddUsersToExamGroup(UsersExamGroupModel model)
        {
            // TODO:
            return this.Json(true);
        }

        public ActionResult RemoveUsersFromExamGroup(UsersExamGroupModel model)
        {
            // TODO:
            return this.Json(true);
        }
    }
}