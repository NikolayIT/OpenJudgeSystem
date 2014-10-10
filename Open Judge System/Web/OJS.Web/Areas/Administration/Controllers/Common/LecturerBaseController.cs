namespace OJS.Web.Areas.Administration.Controllers.Common
{
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Controllers;

    [AuthorizeRoles(SystemRole.Administrator, SystemRole.Lecturer)]
    public abstract class LecturerBaseController : AdministrationController
    {
        protected LecturerBaseController(IOjsData data)
            : base(data)
        {
        }
    }
}