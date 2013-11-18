namespace OJS.Web.Areas.Contests.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class SubmissionModel
    {
        public int ProblemId { get; set; }

        public int SubmissionTypeId { get; set; }

        [StringLength(int.MaxValue, MinimumLength = 5)]
        [Required]
        [AllowHtml]
        public string Content { get; set; }
    }
}