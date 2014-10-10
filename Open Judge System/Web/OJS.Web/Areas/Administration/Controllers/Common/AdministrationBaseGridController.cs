namespace OJS.Web.Areas.Administration.Controllers.Common
{
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Controllers;

    [AuthorizeRoles(SystemRole.Administrator)]
    public abstract class AdministrationBaseGridController : KendoGridAdministrationController
    {
        protected AdministrationBaseGridController(IOjsData data)
            : base(data)
        {
        }
    }
}