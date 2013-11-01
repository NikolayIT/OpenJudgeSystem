namespace OJS.Web.Areas.Contests.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class ProblemResultViewModel
    {
        [Display(Name = "Участник")]
        public string ParticipantName { get; set; }

        [Display(Name = "Резултат")]
        public int Result { get; set; }

        public int MaximumPoints { get; set; }

        public int ProblemId { get; set; }
    }
}