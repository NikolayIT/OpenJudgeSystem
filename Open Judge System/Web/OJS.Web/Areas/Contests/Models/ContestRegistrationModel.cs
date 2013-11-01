namespace OJS.Web.Areas.Contests.Models
{
    using System.Collections.Generic;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.ViewModels;

    public class ContestRegistrationModel
    {
        public ContestRegistrationModel()
        {
            this.Questions = new HashSet<ContestQuestionAnswerModel>();
        }

        public string Password { get; set; }

        public IEnumerable<ContestQuestionAnswerModel> Questions { get; set; }
    }
}
