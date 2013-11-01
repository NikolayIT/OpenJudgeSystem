namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Linq.Expressions;
    
    using OJS.Data.Models;
    
    public class ContestProblemResourceViewModel
    {
        public static Expression<Func<ProblemResource, ContestProblemResourceViewModel>> FromResource
        {
            get
            {
                return resource => new ContestProblemResourceViewModel
                {
                    ResourceId = resource.Id,
                    Name = resource.Name,
                    Link = resource.Link,
                    ProblemType = resource.Type
                };
            }
        }

        public int ResourceId { get; set; }

        public string Name { get; set; }

        public string Link { get; set; }

        public ProblemResourceType ProblemType { get; set; }
    }
}