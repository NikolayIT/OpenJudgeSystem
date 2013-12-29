namespace OJS.Web.Areas.Administration.ViewModels.Roles
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Common.DataAnnotations;
    using OJS.Web.Areas.Administration.ViewModels.Common;

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
        
        [HiddenInput(DisplayValue = false)]
        public string RoleId { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително!")]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        public override IdentityRole GetEntityModel(IdentityRole model = null)
        {
            model = model ?? new IdentityRole();
            model.Id = this.RoleId;
            model.Name = this.Name;
            return model;
        }
    }
}