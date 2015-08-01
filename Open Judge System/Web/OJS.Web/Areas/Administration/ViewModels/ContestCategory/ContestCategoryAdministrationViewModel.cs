namespace OJS.Web.Areas.Administration.ViewModels.ContestCategory
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

    public class ContestCategoryAdministrationViewModel : AdministrationViewModel<ContestCategory>
    {
        [ExcludeFromExcel]
        public static Expression<Func<ContestCategory, ContestCategoryAdministrationViewModel>> ViewModel
        {
            get
            {
                return category => new ContestCategoryAdministrationViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    OrderBy = category.OrderBy,
                    IsVisible = category.IsVisible,
                    CreatedOn = category.CreatedOn,
                    ModifiedOn = category.ModifiedOn
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
        [Required(ErrorMessage = "Името е задължително!")]
        [StringLength(
            GlobalConstants.ContestCategoryNameMaxLength, 
            MinimumLength = GlobalConstants.ContestCategoryNameMinLength, 
            ErrorMessage = "Позволената дължина е между 6 и 100 символа")]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Подредба")]
        [Required(ErrorMessage = "Подредбата е задължителна!")]
        [UIHint("Integer")]
        public int OrderBy { get; set; }

        [DatabaseProperty]
        [Display(Name = "Видимост")]
        public bool IsVisible { get; set; }
    }
}