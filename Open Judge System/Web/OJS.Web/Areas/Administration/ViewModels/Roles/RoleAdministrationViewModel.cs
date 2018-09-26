namespace OJS.Web.Areas.Administration.ViewModels.Roles
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity.EntityFramework;
    using OJS.Common.DataAnnotations;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.Roles.ViewModels.RolesViewModels;

    public class RoleAdministrationViewModel : AdministrationViewModel<IdentityRole>
    {
        [ExcludeFromExcel]
        public static Expression<Func<IdentityRole, RoleAdministrationViewModel>> ViewModel
        {
            get
            {
                return role => new RoleAdministrationViewModel
                {
                    RoleId = role.Id,
                    Name = role.Name,
                };
            }
        }

        [DatabaseProperty(Name = "Id")]
        [HiddenInput(DisplayValue = false)]
        public string RoleId { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Name_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string Name { get; set; }
    }
}