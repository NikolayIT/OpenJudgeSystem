namespace OJS.Web.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Newtonsoft.Json;

    using NPOI.HSSF.UserModel;

    using OJS.Common.DataAnnotations;
    using OJS.Data;
    using OJS.Web.Common.Interfaces;

    public abstract class KendoGridAdministrationController : AdministrationController, IKendoGridAdministrationController
    {
        protected KendoGridAdministrationController(IOjsData data)
            : base(data)
        {
        }

        public abstract IEnumerable GetData();

        public abstract object GetById(object id);

        public virtual string GetEntityKeyName()
        {
            return null;
        }

        [HttpPost]
        public virtual ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            var data = this.GetData();
            var serializationSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var json = JsonConvert.SerializeObject(data.ToDataSourceResult(request), Formatting.None, serializationSettings);
            return this.Content(json, "application/json");
        }

        [HttpGet]
        public FileResult ExportToExcel([DataSourceRequest] DataSourceRequest request)
        {
            return this.ExportToExcel(request, this.GetData());
        }

        protected object BaseCreate(object model)
        {
            if (model != null && this.ModelState.IsValid)
            {
                var itemForAdding = this.Data.Context.Entry(model);
                itemForAdding.State = EntityState.Added;
                this.Data.SaveChanges();
                var databaseValues = itemForAdding.GetDatabaseValues();
                return databaseValues[this.GetEntityKeyName()];
            }

            return null;
        }

        protected void BaseUpdate(object model)
        {
            if (model != null && this.ModelState.IsValid)
            {
                var itemForUpdating = this.Data.Context.Entry(model);
                itemForUpdating.State = EntityState.Modified;
                this.Data.SaveChanges();
            }
        }

        protected void BaseDestroy(object id)
        {
            var model = this.GetById(id);
            if (model != null)
            {
                var itemForDeletion = this.Data.Context.Entry(model);
                if (itemForDeletion != null)
                {
                    itemForDeletion.State = EntityState.Deleted;
                    this.Data.SaveChanges();
                }
            }
        }

        [NonAction]
        protected JsonResult GridOperation([DataSourceRequest]DataSourceRequest request, object model)
        {
            return this.Json(new[] { model }.ToDataSourceResult(request, this.ModelState));
        }

        protected string GetEntityKeyNameByType(Type type)
        {
            return type.GetProperties()
                .FirstOrDefault(pr => pr.GetCustomAttributes(typeof(KeyAttribute), true).Any())
                .Name;
        }
    }
}