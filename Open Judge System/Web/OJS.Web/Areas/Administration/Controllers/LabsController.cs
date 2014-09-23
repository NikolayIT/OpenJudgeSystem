namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Controllers;

    public class LabsController : BaseController
    {
        public LabsController(IOjsData data)
            : base(data)
        {
        }

        public LabsController(IOjsData data, UserProfile profile)
            : base(data, profile)
        {
        }

        public ActionResult Index()
        {
            return this.View();
        }
    }
}