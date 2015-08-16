namespace OJS.Web.Areas.Administration.ViewModels.Setting
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    using Resource = Resources.Areas.Administration.Settings.ViewModels.SettingAdministration;

    public class SettingAdministrationViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<Setting, SettingAdministrationViewModel>> ViewModel
        {
            get
            {
                return set => new SettingAdministrationViewModel
                {
                    Name = set.Name,
                    Value = set.Value,
                };
            }
        }

        [DatabaseProperty]
        [Display(Name = "Name", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Name_required",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Value", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Value_required",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("MultiLineText")]
        public string Value { get; set; }

        public Setting GetEntityModel(Setting model = null)
        {
            model = model ?? new Setting();
            model.Name = this.Name;
            model.Value = this.Value;
            return model;
        }
    }
}