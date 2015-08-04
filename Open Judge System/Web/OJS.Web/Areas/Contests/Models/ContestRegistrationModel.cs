namespace OJS.Web.Areas.Contests.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Data;

    public class ContestRegistrationModel : IValidatableObject
    {
        private IOjsData data;

        public ContestRegistrationModel() : this(new OjsData())
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

            var contest = this.data.Contests.GetById(this.ContestId);
            var contestQuestions = contest.Questions
                .Where(x => !x.IsDeleted)
                .ToList();

            var counter = 0;
            foreach (var question in contestQuestions)
            {
                var answer = this.Questions.FirstOrDefault(x => x.QuestionId == question.Id);
                var memberName = string.Format("Questions[{0}].Answer", counter);
                if (answer == null)
                {
                    var validationErrorMessage = string.Format("Question with id {0} was not answered.", question.Id);
                    var validationResult = new ValidationResult(validationErrorMessage, new[] { memberName });
                    validationResults.Add(validationResult);
                }
                else if (!string.IsNullOrWhiteSpace(question.RegularExpressionValidation) && !Regex.IsMatch(answer.Answer, question.RegularExpressionValidation))
                {
                    var validationErrorMessage = string.Format("Question with id {0} is not in the correct format.", question.Id);
                    var validationResult = new ValidationResult(validationErrorMessage, new[] { memberName });
                    validationResults.Add(validationResult);
                }

                counter++;
            }

            return validationResults;
        }
    }
}
