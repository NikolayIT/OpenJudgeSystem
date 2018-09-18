namespace OJS.Services.Cache.Models
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using OJS.Common.Extensions;
    using OJS.Data.Models;

    public class ContestCategoryListViewModel
    {
        public static Expression<Func<ContestCategory, ContestCategoryListViewModel>> FromCategory
        {
            get
            {
                return category => new ContestCategoryListViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    HasChildren = category.Children.Any(x => x.IsVisible && !x.IsDeleted)
                };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string NameUrl => this.Name.ToUrl();

        public bool HasChildren { get; set; }
    }
}