namespace OJS.Web.Areas.Administration.ViewModels.AntiCheat
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using OJS.Common.Models;

    public class SubmissionSimilarityFiltersInputModel
    {
        [Required]
        [Display(Name = "Състезание")]
        public int? ContestId { get; set; }

        [Display(Name = "Тип детектор")]
        public PlagiarismDetectorType PlagiarismDetectorType { get; set; }

        public IEnumerable<SelectListItem> PlagiarismDetectorTypes { get; set; }
    }
}