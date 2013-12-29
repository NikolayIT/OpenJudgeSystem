namespace OJS.Web.Areas.Administration.ViewModels.Common
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Web.Common.Interfaces;

    public abstract class AdministrationViewModel<T> : IAdministrationViewModel<T> where T : class
    {
        [DatabaseProperty]
        [Display(Name = "Дата на създаване")]
        [DataType(DataType.DateTime)]
        [HiddenInput(DisplayValue = false)]
        public DateTime? CreatedOn { get; set; }

        [DatabaseProperty]
        [Display(Name = "Дата на промяна")]
        [DataType(DataType.DateTime)]
        [HiddenInput(DisplayValue = false)]
        public DateTime? ModifiedOn { get; set; }

        public abstract T GetEntityModel(T model = null);

        protected T ConvertToDatabaseEntity(T model)
        {
            foreach (var property in this.GetType().GetProperties())
            {
                if (property.GetCustomAttributes(typeof(DatabasePropertyAttribute), true).Any())
                {
                    model.GetType().GetProperty(property.Name).SetValue(model, property.GetValue(this));
                }
            }

            return model;
        }
    }
}