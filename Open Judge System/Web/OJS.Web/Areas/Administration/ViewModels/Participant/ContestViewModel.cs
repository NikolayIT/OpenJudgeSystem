namespace OJS.Web.Areas.Administration.ViewModels.Participant
{
    using System;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    public class ContestViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<Contest, ContestViewModel>> ViewModel
        {
            get
            {
                return c => new ContestViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }
    }
}