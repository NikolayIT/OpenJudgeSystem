namespace OJS.Web.Areas.Administration.ViewModels.Setting
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    using static OJS.Common.Constants.EditorTemplateConstants;

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
        [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Name_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Value), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Value_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(MultiLineText)]
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