namespace OJS.Web.Controllers
{
    using System.Web.Mvc;
    using OJS.Data;

    [Authorize(Roles = "Administrator")]
    public class AdministrationController : BaseController
    {
        public AdministrationController(IOjsData data)
            : base(data)
        {
        }
    }
}