namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers.Common;

    public class ExamGroupsController : LecturerBaseController
    {
        public ExamGroupsController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index()
        {
            return this.View();
        }
    }
}