namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.Controllers;

    public class LecturersInContestsController : KendoGridAdministrationController
    {
        // GET: Administration/LecturersInContests
        public LecturersInContestsController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index()
        {
            return this.View();
        }

        public override IEnumerable GetData()
        {
            throw new NotImplementedException();
        }

        public override object GetById(object id)
        {
            throw new NotImplementedException();
        }
    }
}