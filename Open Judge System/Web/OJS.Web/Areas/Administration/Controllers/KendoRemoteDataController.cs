namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Common.Attributes;

    public class KendoRemoteDataController : KendoRemoteDataBaseController
    {
        private readonly IUsersDataService usersData;

        public KendoRemoteDataController(
            IOjsData data,
            IUsersDataService usersData)
            : base(data) =>
                this.usersData = usersData;

        [AjaxOnly]
        public JsonResult GetUsersContaining(string userFilter)
        {
            var users = this.usersData.GetAll().Take(DefaultItemsToTake);

            if (!string.IsNullOrWhiteSpace(userFilter))
            {
                users = this.usersData
                    .GetAll()
                    .Where(u => u.UserName.ToLower().Contains(userFilter.ToLower()))
                    .Take(DefaultItemsToTake);
            }

            var result = users
                .Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.Id
                })
                .ToList();

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}