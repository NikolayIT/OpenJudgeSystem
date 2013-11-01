namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Models;

    public class ContestRegistrationViewModel
    {
        public ContestRegistrationViewModel(Contest contest, bool isOfficial)
        {
            this.ContestName = contest.Name;

            if (isOfficial)
            {
                this.RequirePassword = contest.HasContestPassword;
            }
            else
            {
                this.RequirePassword = contest.HasPracticePassword;
            }

            this.Questions = contest.Questions.AsQueryable().Select(QuestionViewModel.FromQuestion);
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
                    Question = x.Question
                };
            });
        }

        public string ContestName { get; set; }

        public bool RequirePassword { get; set; }

        [Display(Name = "Парола")]
        public string Password { get; set; }

        public IEnumerable<QuestionViewModel> Questions { get; set; }
    }
}