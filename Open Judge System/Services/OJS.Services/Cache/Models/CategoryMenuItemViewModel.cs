namespace OJS.Services.Cache.Models
{
    using System;
    using System.Linq.Expressions;

    using OJS.Common.Extensions;
    using OJS.Data.Models;

    public class CategoryMenuItemViewModel
    {
        public static Expression<Func<ContestCategory, CategoryMenuItemViewModel>> FromCategory
        {
            get
            {
                return category => new CategoryMenuItemViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string NameUrl => this.Name.ToUrl();
    }
}