namespace OJS.Web.Controllers
{
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Data;

    [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
    public class AdministrationController : BaseController
    {
        public AdministrationController(IOjsData data)
            : base(data)
        {
        }
    }
}