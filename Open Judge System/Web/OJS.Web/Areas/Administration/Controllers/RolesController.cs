namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;
    using Kendo.Mvc.Extensions;

    using OJS.Data;
    using OJS.Web.Controllers;

    using ModelType = OJS.Web.Areas.Administration.ViewModels.Roles.RoleAdministrationViewModel;
    using DetailModelType = OJS.Web.Areas.Administration.ViewModels.Roles.UserInRoleAdministrationViewModel;
    using System;
    using OJS.Web.Areas.Administration.ViewModels.Roles;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class RolesController : KendoGridAdministrationController
    {
        public RolesController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Roles
                .All()
                .Select(ModelType.ViewModel);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            model.RoleId = Guid.NewGuid().ToString();

            return this.BaseCreate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseUpdate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseDestroy(request, model.ToEntity);
        }

        [HttpPost]
        public JsonResult UsersInRole([DataSourceRequest]DataSourceRequest request, string id)
        {
            var users = this.Data.Users
                .All()
                .Where(u => u.Roles.Any(r => r.RoleId == id))
                .Select(DetailModelType.ViewModel);

            return Json(users.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AvailableUsersForRole(string text)
        {
            var users = this.Data.Users.All();

            if (!string.IsNullOrEmpty(text))
            {
                users = users.Where(u => u.UserName.ToLower().Contains(text.ToLower()));
            }

            var result = users
                .ToList()
                .Select(pr => new SelectListItem
                {
                    Text = pr.UserName,
                    Value = pr.Id,
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddUserToRole([DataSourceRequest]DataSourceRequest request, string id, string userId)
        {
            var user = this.Data.Users.GetById(userId);
            var role = this.Data.Roles.All().FirstOrDefault(r => r.Id == id);

            user.Roles.Add(new IdentityUserRole { RoleId = role.Id, UserId = userId });
            this.Data.SaveChanges();

            var result = new UserInRoleAdministrationViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                FirstName = user.UserSettings.FirstName,
                LastName = user.UserSettings.LastName,
                Email = user.Email
            };

            return Json(new[] { result }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteUserFromRole([DataSourceRequest]DataSourceRequest request, string id, DetailModelType model)
        {
            var user = this.Data.Users.GetById(model.UserId);
            var role = user.Roles.FirstOrDefault(r => r.RoleId == id);

            user.Roles.Remove(role);
            this.Data.SaveChanges();

            return Json(ModelState.ToDataSourceResult());
        }
    }
}