namespace OJS.Web.Areas.Administration.InputModels.AntiCheat
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using OJS.Web.ViewModels.Common;
    using OJS.Workers.Common.Models;

    public class SubmissionSimilarityFiltersInputModel
    {
        [Required]
        [Display(Name = "Състезание")]
        public int? ContestId { get; set; }

        [Display(Name = "Тип детектор")]
        public PlagiarismDetectorType PlagiarismDetectorType { get; set; }

        public IEnumerable<DropdownViewModel> PlagiarismDetectorTypes { get; set; }
    }
}