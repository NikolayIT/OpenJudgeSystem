namespace OJS.Web.Areas.Administration.ViewModels.Checker
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using Resource = Resources.Areas.Administration.Checkers.ViewModels.CheckerAdministrationViewModel;

    public class CheckerAdministrationViewModel : AdministrationViewModel<Checker>
    {
        [ExcludeFromExcel]
        public static Expression<Func<Checker, CheckerAdministrationViewModel>> ViewModel
        {
            get
            {
                return ch => new CheckerAdministrationViewModel
                {
                    Id = ch.Id,
                    Name = ch.Name,
                    Description = ch.Description,
                    DllFile = ch.DllFile,
                    ClassName = ch.ClassName,
                    Parameter = ch.Parameter,
                    CreatedOn = ch.CreatedOn,
                    ModifiedOn = ch.ModifiedOn,
                };
            }
        }

        [DatabaseProperty]
        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [DatabaseProperty]
        [Display(Name = "Name", ResourceType = typeof(Resource))]
        [Required(
            AllowEmptyStrings = false, 
            ErrorMessageResourceType = typeof(Resource), 
            ErrorMessageResourceName = "Name_required")]
        [StringLength(
            GlobalConstants.CheckerNameMaxLength, 
            MinimumLength = GlobalConstants.CheckerNameMinLength,
            ErrorMessageResourceType = typeof(Resource),
            ErrorMessageResourceName = "Name_length")]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Description", ResourceType = typeof(Resource))]
        [UIHint("MultiLineText")]
        public string Description { get; set; }

        [DatabaseProperty]
        [Display(Name = "Dll_file", ResourceType = typeof(Resource))]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceType = typeof(Resource),
            ErrorMessageResourceName = "Dll_file_required")]
        [UIHint("SingleLineText")]
        public string DllFile { get; set; }

        [DatabaseProperty]
        [Display(Name = "Class_name", ResourceType = typeof(Resource))]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceType = typeof(Resource),
            ErrorMessageResourceName = "Class_name_required")]
        [UIHint("SingleLineText")]
        public string ClassName { get; set; }

        [AllowHtml]
        [DatabaseProperty]
        [Display(Name = "Param", ResourceType = typeof(Resource))]
        [UIHint("MultiLineText")]
        public string Parameter { get; set; }
    }
}