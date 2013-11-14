namespace OJS.Web.Areas.Administration.ViewModels.ContestCategories
{
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    public class ContestCategoryAdministrationViewModel : AdministrationViewModel
    {
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

        public ContestCategory ToEntity
        {
            get
            {
                return new ContestCategory
                {
                    Id = this.Id ?? default(int),
                    Name = this.Name,
                    OrderBy = this.OrderBy,
                    IsVisible = this.IsVisible,
                    CreatedOn = this.CreatedOn,
                    ModifiedOn = this.ModifiedOn
                };
            }
        }

        [Display(Name = "№")]
        [DefaultValue(null)]
        [UIHint("_NonEditable")]
        public int? Id { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително!")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Позволената дължина е между 6 и 100 символа")]
        [UIHint("_SingleLineText")]
        public string Name { get; set; }

        [Display(Name = "Подредба")]
        [Required(ErrorMessage = "Подредбата е задължителна!")]
        [UIHint("_OrderBy")]
        public int OrderBy { get; set; }

        [Display(Name = "Видимост")]
        public bool IsVisible { get; set; }
    }
}