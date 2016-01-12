namespace OJS.Web.Controllers
{
    using System;
    using System.Collections;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Newtonsoft.Json;

    using OJS.Common;
    using OJS.Data;
    using OJS.Web.Common.Interfaces;

    public abstract class KendoGridAdministrationController : AdministrationController, IKendoGridAdministrationController
    {
        private const string CreatedOnPropertyName = "CreatedOn";
        private const string ModifiedOnPropertyName = "ModifiedOn";

        protected KendoGridAdministrationController(IOjsData data)
            : base(data)
        {
        }

        public abstract IEnumerable GetData();

        public abstract object GetById(object id);

        public virtual string GetEntityKeyName()
        {
            throw new InvalidOperationException("GetEntityKeyName method required but not implemented in derived controller");
        }

        [HttpPost]
        public virtual ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            var data = this.GetData();
            var serializationSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var json = JsonConvert.SerializeObject(data.ToDataSourceResult(request), Formatting.None, serializationSettings);
            return this.Content(json, GlobalConstants.JsonMimeType);
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

        protected void UpdateAuditInfoValues<T>(IAdministrationViewModel<T> viewModel, object databaseModel)
            where T : class, new()
        {
            var entry = this.Data.Context.Entry(databaseModel);
            viewModel.CreatedOn = (DateTime?)entry.Property(CreatedOnPropertyName).CurrentValue;
            viewModel.ModifiedOn = (DateTime?)entry.Property(ModifiedOnPropertyName).CurrentValue;
        }
    }
}