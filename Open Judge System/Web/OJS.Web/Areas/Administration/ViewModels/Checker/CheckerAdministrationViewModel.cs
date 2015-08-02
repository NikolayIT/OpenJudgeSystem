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
        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително!", AllowEmptyStrings = false)]
        [StringLength(
            GlobalConstants.CheckerNameMaxLength, 
            MinimumLength = GlobalConstants.CheckerNameMinLength)]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Описание")]
        [UIHint("MultiLineText")]
        public string Description { get; set; }

        [DatabaseProperty]
        [Display(Name = "DLL файл")]
        [Required(ErrorMessage = "DLL файла е задължителен!", AllowEmptyStrings = false)]
        [UIHint("SingleLineText")]
        public string DllFile { get; set; }

        [DatabaseProperty]
        [Display(Name = "Име на клас")]
        [Required(ErrorMessage = "Името на класа е задължително!", AllowEmptyStrings = false)]
        [UIHint("SingleLineText")]
        public string ClassName { get; set; }

        [AllowHtml]
        [DatabaseProperty]
        [Display(Name = "Параметър")]
        [UIHint("MultiLineText")]
        public string Parameter { get; set; }
    }
}