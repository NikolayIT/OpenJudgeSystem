namespace OJS.Web.Areas.Administration.ViewModels.ContestCategory
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    public class ContestCategoryAdministrationViewModel : AdministrationViewModel
    {
        [ExcludeFromExcelAttribute]
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

        [ExcludeFromExcelAttribute]
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
                    CreatedOn = this.CreatedOn.GetValueOrDefault(),
                    ModifiedOn = this.ModifiedOn
                };
            }
        }

        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително!")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Позволената дължина е между 6 и 100 символа")]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [Display(Name = "Подредба")]
        [Required(ErrorMessage = "Подредбата е задължителна!")]
        [UIHint("PositiveInteger")]
        public int OrderBy { get; set; }

        [Display(Name = "Видимост")]
        public bool IsVisible { get; set; }
    }
}