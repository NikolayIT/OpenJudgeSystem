namespace OJS.Web.Areas.Contests.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Web;

    public class BinarySubmissionModel
    {
        public int ProblemId { get; set; }

        public int SubmissionTypeId { get; set; }

        [Required]
        public HttpPostedFileBase File { get; set; }
    }
}
