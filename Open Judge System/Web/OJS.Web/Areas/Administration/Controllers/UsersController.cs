namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers.Common;

    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.User.UserProfileAdministrationViewModel;

    public class UsersController : AdministrationBaseGridController
    {
        public UsersController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Users
                .All()
                .Select(ViewModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.Users
                .All()
                .FirstOrDefault(o => o.Id == (string)id);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var list = new List<ViewModelType>();

            if (model != null && this.ModelState.IsValid)
            {
                var userProfile = this.Data.Users.All().FirstOrDefault(u => u.Id == model.Id);
                var itemForUpdating = this.Data.Context.Entry(model.GetEntityModel(userProfile));
                itemForUpdating.State = EntityState.Modified;
                this.Data.SaveChanges();
                list.Add(model);
            }

            return this.Json(list.ToDataSourceResult(request));
        }
    }
}