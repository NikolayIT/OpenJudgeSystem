namespace OJS.Web.Areas.Administration.ViewModels.Participant
{
    using System;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    public class UserViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<UserProfile, UserViewModel>> ViewModel
        {
            get
            {
                return c => new UserViewModel
                {
                    Id = c.Id,
                    Name = c.UserName
                };
            }
        }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}