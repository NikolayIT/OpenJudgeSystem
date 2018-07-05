namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System;
    using System.Linq.Expressions;

    using OJS.Common.Models;
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
                    RawLink = resource.Link,
                    ProblemType = resource.Type
                };
            }
        }

        public int ResourceId { get; set; }

        public string Name { get; set; }

        public string RawLink { get; set; }

        public ProblemResourceType ProblemType { get; set; }

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