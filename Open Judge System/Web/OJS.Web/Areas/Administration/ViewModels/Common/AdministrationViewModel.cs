namespace OJS.Web.Areas.Administration.ViewModels.Common
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Web.Common.Interfaces;

    using Resource = Resources.Areas.Administration.AdministrationGeneral;

    public abstract class AdministrationViewModel<T> : IAdministrationViewModel<T>
        where T : class, new()
    {
        [Display(Name = "Created_on", ResourceType = typeof(Resource))]
        [DataType(DataType.DateTime)]
        [HiddenInput(DisplayValue = false)]
        public DateTime? CreatedOn { get; set; }

        [Display(Name = "Modified_on", ResourceType = typeof(Resource))]
        [DataType(DataType.DateTime)]
        [HiddenInput(DisplayValue = false)]
        public DateTime? ModifiedOn { get; set; }

        public virtual T GetEntityModel(T model = null)
        {
            model = model ?? new T();
            return this.ConvertToDatabaseEntity(model);
        }

        protected T ConvertToDatabaseEntity(T model)
        {
            foreach (var viewModelProperty in this.GetType().GetProperties())
            {
                var customAttributes = viewModelProperty.GetCustomAttributes(typeof(DatabasePropertyAttribute), true);

                if (customAttributes.Any())
                {
                    var name = (customAttributes.First() as DatabasePropertyAttribute).Name;

                    if (string.IsNullOrEmpty(name))
                    {
                        name = viewModelProperty.Name;
                    }

                    var databaseEntityProperty = model.GetType().GetProperties().FirstOrDefault(pr => pr.Name == name);

                    databaseEntityProperty?.SetValue(model, viewModelProperty.GetValue(this));
                }
            }

            return model;
        }
    }
}