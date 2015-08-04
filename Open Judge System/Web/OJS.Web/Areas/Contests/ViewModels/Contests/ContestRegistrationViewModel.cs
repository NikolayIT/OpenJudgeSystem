namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Models;

    using Resource = Resources.Areas.Contests.ViewModels.ContestsViewModels;

    public class ContestRegistrationViewModel
    {
        public ContestRegistrationViewModel(Contest contest, bool isOfficial)
        {
            this.ContestName = contest.Name;
            this.ContestId = contest.Id;

            this.RequirePassword = isOfficial ? contest.HasContestPassword : contest.HasPracticePassword;

            this.Questions = contest.Questions
                .Where(x => !x.IsDeleted)
                .AsQueryable()
                .Select(QuestionViewModel.FromQuestion);
        }

        public ContestRegistrationViewModel(Contest contest, ContestRegistrationModel userAnswers, bool isOfficial)
            : this(contest, isOfficial)
        {
            this.Questions = this.Questions.Select(x =>
            {
                var userAnswer = userAnswers.Questions.FirstOrDefault(y => y.QuestionId == x.QuestionId);
                return new QuestionViewModel
                {
                    Answer = userAnswer == null ? null : userAnswer.Answer,
                    QuestionId = x.QuestionId,
                    Question = x.Question,
                    Type = x.Type,
                    PossibleAnswers = x.PossibleAnswers
                };
            });
        }

        public int ContestId { get; set; }

        public string ContestName { get; set; }

        public bool RequirePassword { get; set; }

        [Display(Name = "Password", ResourceType = typeof(Resource))]
        public string Password { get; set; }

        public IEnumerable<QuestionViewModel> Questions { get; set; }
    }
}