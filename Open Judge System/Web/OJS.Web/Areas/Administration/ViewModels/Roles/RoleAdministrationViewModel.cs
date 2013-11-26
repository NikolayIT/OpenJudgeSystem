namespace OJS.Web.Areas.Administration.ViewModels.Roles
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Common.DataAnnotations;

    public class RoleAdministrationViewModel
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

        [ExcludeFromExcel]
        public IdentityRole ToEntity
        {
            get
            {
                return new IdentityRole
                {
                    Id = this.RoleId,
                    Name = this.Name,
                };
            }
        }
        
        [HiddenInput(DisplayValue = false)]
        public string RoleId { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително!")]
        [UIHint("SingleLineText")]
        public string Name { get; set; }
    }
}