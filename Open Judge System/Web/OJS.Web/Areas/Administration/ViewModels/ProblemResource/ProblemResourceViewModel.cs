namespace OJS.Web.Areas.Administration.ViewModels.ProblemResource
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data.Models;

    using Resource = Resources.Areas.Administration.Problems.ViewModels.ProblemResources;

    public class ProblemResourceViewModel
    {
        public static Expression<Func<ProblemResource, ProblemResourceViewModel>> FromProblemResource
        {
            get
            {
                return res => new ProblemResourceViewModel
                {
                    Id = res.Id,
                    Name = res.Name,
                    Type = res.Type,
                    OrderBy = res.OrderBy,
                    ProblemId = res.ProblemId,
                    ProblemName = res.Problem.Name,
                    RawLink = res.Link
                };
            }
        }

        public int? Id { get; set; }

        [Display(Name = "Name", ResourceType = typeof(Resource))]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceName = "Name_required",
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.ProblemResourceNameMaxLength,
            MinimumLength = GlobalConstants.ProblemResourceNameMinLength,
            ErrorMessageResourceName = "Name_length",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue("Име")]
        public string Name { get; set; }

        public int ProblemId { get; set; }

        public string ProblemName { get; set; }

        [Display(Name = "Type", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Type_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(ProblemResourceType.ProblemDescription)]
        public ProblemResourceType Type { get; set; }

        public int DropDownTypeIndex => (int)this.Type - 1;

        public IEnumerable<SelectListItem> AllTypes { get; set; }

        public HttpPostedFileBase File { get; set; }

        public string FileExtension
        {
            get
            {
                var fileName = this.File.FileName;
                return fileName.Substring(fileName.LastIndexOf('.') + 1);
            }
        }

        [Display(Name = "Link", ResourceType = typeof(Resource))]
        [DefaultValue("http://")]
        public string RawLink { get; set; }

        [Display(Name = "Order", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Order_required",
            ErrorMessageResourceType = typeof(Resource))]
        public int OrderBy { get; set; }

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