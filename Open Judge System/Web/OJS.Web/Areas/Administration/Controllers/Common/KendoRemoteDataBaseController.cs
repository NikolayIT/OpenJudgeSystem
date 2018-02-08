namespace OJS.Web.Areas.Administration.Controllers.Common
{
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Controllers;

    [AuthorizeRoles(SystemRole.Administrator, SystemRole.Lecturer)]
    public class KendoRemoteDataBaseController : AdministrationController
    {
        protected const int DefaultItemsToTake = 20;

        public KendoRemoteDataBaseController(IOjsData data)
            : base(data)
        {
        }
    }
}