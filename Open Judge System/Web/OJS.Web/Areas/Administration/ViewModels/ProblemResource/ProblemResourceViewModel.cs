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
                };
            }
        }

        public int? Id { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името на ресурса е задължително!", AllowEmptyStrings = false)]
        [StringLength(
            GlobalConstants.ProblemResourceNameMaxLength, 
            MinimumLength = GlobalConstants.ProblemResourceNameMinLength)]
        [DefaultValue("Име")]
        public string Name { get; set; }

        public int ProblemId { get; set; }

        public string ProblemName { get; set; }

        [Display(Name = "Тип")]
        [Required(ErrorMessage = "Типа е задължителен!")]
        [DefaultValue(ProblemResourceType.ProblemDescription)]
        public ProblemResourceType Type { get; set; }

        public int DropDownTypeIndex
        {
            get
            {
                return (int)this.Type - 1;
            }
        }

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

        [Display(Name = "Линк")]
        [DefaultValue("http://")]
        public string Link { get; set; }

        [Display(Name = "Подредба")]
        [Required(ErrorMessage = "Подредбата е задължителна!")]
        public int OrderBy { get; set; }
    }
}