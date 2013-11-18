namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Controllers;

    using ModelType = OJS.Web.Areas.Administration.ViewModels.User.UserProfileAdministrationViewModel;

    public class UsersController : KendoGridAdministrationController
    {
        public UsersController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Users
                .All()
                .Select(ModelType.ViewModel);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            var list = new List<ModelType>();

            if (model != null && ModelState.IsValid)
            {
                var itemForUpdating = this.Data.Context.Entry(model.ToEntity);
                itemForUpdating.State = EntityState.Modified;
                this.Data.SaveChanges();
                list.Add(model);
            }

            return this.Json(list.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            throw new NotImplementedException();
        }
    }
}