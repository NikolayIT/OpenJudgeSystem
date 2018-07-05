namespace OJS.Web.Areas.Administration.ViewModels.ProblemResource
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data.Models;

    using Resource = Resources.Areas.Administration.Problems.ViewModels.ProblemResources;

    public class ProblemResourceGridViewModel
    {
        [ExcludeFromExcel]
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
                    RawLink = resource.Link,
                    OrderBy = resource.OrderBy,
                };
            }
        }

        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        public int ProblemId { get; set; }

        [Display(Name = "Name", ResourceType = typeof(Resource))]
        public string Name { get; set; }

        [Display(Name = "Type", ResourceType = typeof(Resource))]
        public ProblemResourceType Type { get; set; }

        [Display(Name = "Order", ResourceType = typeof(Resource))]
        public int OrderBy { get; set; }

        [Display(Name = "Link", ResourceType = typeof(Resource))]
        public string RawLink { get; set; }

        public string TypeName => this.Type.GetDescription();

        public bool IsLinkFromSvn =>
            !string.IsNullOrWhiteSpace(this.RawLink) && this.RawLink.StartsWith(Settings.SvnBaseUrl, StringComparison.OrdinalIgnoreCase);

        public string Link
        {
            get
            {
                if (this.IsLinkFromSvn)
                {
                    return this.RawLink.Replace(Settings.SvnBaseUrl, Settings.LearningSystemSvnDownloadBaseUrl);
                }

                return this.RawLink;
            }
        }
    }
}