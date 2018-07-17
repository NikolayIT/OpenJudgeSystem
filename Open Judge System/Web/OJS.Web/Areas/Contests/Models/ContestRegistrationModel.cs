namespace OJS.Web.Areas.Contests.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Data;

    using Resource = Resources.Areas.Contests.ViewModels.ContestsViewModels;

    public class ContestRegistrationModel : IValidatableObject
    {
        private IOjsData data;

        public ContestRegistrationModel()
            : this(new OjsData())
        {
        }

        public ContestRegistrationModel(IOjsData data)
        {
            this.Questions = new HashSet<ContestQuestionAnswerModel>();
            this.data = data;
        }

        public int ContestId { get; set; }

        public string Password { get; set; }

        public IEnumerable<ContestQuestionAnswerModel> Questions { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new HashSet<ValidationResult>();

            var contestQuestions = this.data.ContestQuestions
                .All()
                .Where(cc => cc.ContestId == this.ContestId)
                .Where(x => !x.IsDeleted)
                .ToList();

            var counter = 0;
            foreach (var question in contestQuestions)
            {
                var answer = this.Questions.FirstOrDefault(x => x.QuestionId == question.Id);
                var memberName = string.Format("Questions[{0}].Answer", counter);
                if (answer == null)
                {
                    var validationErrorMessage = string.Format(Resource.Question_not_answered, question.Id);
                    var validationResult = new ValidationResult(validationErrorMessage, new[] { memberName });
                    validationResults.Add(validationResult);
                }
                else if (!string.IsNullOrWhiteSpace(question.RegularExpressionValidation) && !Regex.IsMatch(answer.Answer, question.RegularExpressionValidation))
                {
                    var validationErrorMessage = string.Format(Resource.Question_not_answered_correctly, question.Id);
                    var validationResult = new ValidationResult(validationErrorMessage, new[] { memberName });
                    validationResults.Add(validationResult);
                }

                counter++;
            }

            return validationResults;
        }
    }
}
