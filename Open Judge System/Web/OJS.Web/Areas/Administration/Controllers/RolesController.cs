namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Common;
    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers.Common;

    using DatabaseModelType = Microsoft.AspNet.Identity.EntityFramework.IdentityRole;
    using DetailModelType = OJS.Web.Areas.Administration.ViewModels.User.UserProfileSimpleAdministrationViewModel;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.Roles.RoleAdministrationViewModel;

    public class RolesController : AdministrationBaseGridController
    {
        private const string EntityKeyName = "Id";

        public RolesController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Roles
                .All()
                .Select(ViewModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.Roles
                .All()
                .FirstOrDefault(o => o.Id == (string)id);
        }

        public override string GetEntityKeyName()
        {
            return EntityKeyName;
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            model.RoleId = Guid.NewGuid().ToString();
            this.BaseCreate(model.GetEntityModel());
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var entity = this.GetById(model.RoleId) as DatabaseModelType;
            this.BaseUpdate(model.GetEntityModel(entity));
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            this.BaseDestroy(model.RoleId);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public JsonResult UsersInRole([DataSourceRequest]DataSourceRequest request, string id)
        {
            var users = this.Data.Users
                .All()
                .Where(u => u.Roles.Any(r => r.RoleId == id))
                .Select(DetailModelType.FromUserProfile);

            return this.Json(users.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddUserToRole([DataSourceRequest]DataSourceRequest request, string id, string userId)
        {
            var user = this.Data.Users.GetById(userId);
            var role = this.Data.Roles.All().FirstOrDefault(r => r.Id == id);

            user.Roles.Add(new IdentityUserRole { RoleId = role.Id, UserId = userId });
            this.Data.SaveChanges();

            var result = new DetailModelType
            {
                UserId = user.Id,
                Username = user.UserName,
                FirstName = user.UserSettings.FirstName,
                LastName = user.UserSettings.LastName,
                Email = user.Email
            };

            return this.Json(new[] { result }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteUserFromRole([DataSourceRequest]DataSourceRequest request, string id, DetailModelType model)
        {
            var user = this.Data.Users.GetById(model.UserId);

            var roleStore = new RoleStore<IdentityRole>();
            var roleManager = new RoleManager<IdentityRole>(roleStore);
            var role = roleManager.Roles.FirstOrDefault(r => r.Id == id);

            if (role != null && role.Name == GlobalConstants.LecturerRoleName)
            {
                this.Data.LecturersInContests.Delete(x => x.LecturerId == model.UserId);
                this.Data.LecturersInContestCategories.Delete(x => x.LecturerId == model.UserId);
            }

            var userRole = user.Roles.FirstOrDefault(r => r.RoleId == id);
            user.Roles.Remove(userRole);
            this.Data.SaveChanges();

            return this.Json(this.ModelState.ToDataSourceResult());
        }
    }
}