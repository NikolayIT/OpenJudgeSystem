namespace OJS.Web.Areas.Administration.ViewModels.ProblemResource
{
    using System;
    using System.Linq.Expressions;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data.Models;

    public class ProblemResourceGridViewModel
    {
        public static Expression<Func<ProblemResource, ProblemResourceGridViewModel>> FromResource
        {
            get
            {
                return resource => new ProblemResourceGridViewModel
                {
                    Id = resource.Id,
                    ProblemId = resource.ProblemId,
                    Name = resource.Name,
                    Type = resource.Type,
                    Link = resource.Link,
                    OrderBy = resource.OrderBy,
                };
            }
        }

        public int Id { get; set; }

        public int ProblemId { get; set; }

        public string Name { get; set; }

        public ProblemResourceType Type { get; set; }

        public string Link { get; set; }

        public int OrderBy { get; set; }

        public string TypeName
        {
            get
            {
                return this.Type.GetDescription();
            }
        }
    }
}