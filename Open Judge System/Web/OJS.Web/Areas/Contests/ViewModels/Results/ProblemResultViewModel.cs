namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ProblemResultViewModel
    {
        public static Expression<Func<Submission, ProblemResultViewModel>> FromSubmission
        {
            get
            {
                return submission => new ProblemResultViewModel
                                {
                                    ProblemId = submission.Problem.Id,
                                    ParticipantName = submission.Participant.User.UserName,
                                    MaximumPoints = submission.Problem.MaximumPoints,
                                    Result = submission.Points
                                };
            }
        }

        [Display(Name = "Участник")]
        public string ParticipantName { get; set; }

        [Display(Name = "Резултат")]
        public int Result { get; set; }

        public int MaximumPoints { get; set; }

        public int ProblemId { get; set; }
    }
}