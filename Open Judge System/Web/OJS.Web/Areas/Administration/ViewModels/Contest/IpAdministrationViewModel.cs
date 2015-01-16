namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class IpAdministrationViewModel
    {
        public static Expression<Func<Ip, IpAdministrationViewModel>> ViewModel
        {
            get
            {
                return x => new IpAdministrationViewModel { Value = x.Value };
            }
        }

        public string Value { get; set; }
    }
}