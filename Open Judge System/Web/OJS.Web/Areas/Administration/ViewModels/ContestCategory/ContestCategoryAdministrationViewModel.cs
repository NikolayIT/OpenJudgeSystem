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

    using Resource = Resources.Areas.Administration.ContestCategories.ViewModels.ContestCategoryAdministrationViewModel;

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
        [Display(Name = "Name", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Name_required",
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.ContestCategoryNameMaxLength, 
            MinimumLength = GlobalConstants.ContestCategoryNameMinLength, 
            ErrorMessageResourceName = "Name_length",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Order_by", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Order_by_required",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("Integer")]
        public int OrderBy { get; set; }

        [DatabaseProperty]
        [Display(Name = "Visibility", ResourceType = typeof(Resource))]
        public bool IsVisible { get; set; }
    }
}