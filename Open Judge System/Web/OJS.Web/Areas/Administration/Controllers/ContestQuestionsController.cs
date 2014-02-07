namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;
    using Kendo.Mvc.Extensions;

    using OJS.Data;
    using OJS.Web.Controllers;

    using DatabaseModelType = OJS.Data.Models.ContestQuestion;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ContestQuestion.ContestQuestionViewModel;

    public class ContestQuestionsController : KendoGridAdministrationController
    {
        private int contestId;

        public ContestQuestionsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            var questions = this.Data.ContestQuestions
                .All()
                .Where(q => q.ContestId == contestId)
                .Select(ViewModelType.ViewModel);

            return questions;
        }

        public override object GetById(object id)
        {
            var question = this.Data.ContestQuestions
                .All()
                .FirstOrDefault(q => q.Id == (int)id);

            return question;
        }

        [HttpPost]
        public JsonResult QuestionsInContest([DataSourceRequest]DataSourceRequest request, int id)
        {
            this.contestId = id;
            var questions = this.GetData();

            return this.Json(questions.ToDataSourceResult(request));
        }

        [HttpPost]
        public JsonResult AddQuestionToContest([DataSourceRequest]DataSourceRequest request, ViewModelType model, int id)
        {
            var contest = this.Data.Contests.All().FirstOrDefault(c => c.Id == id);
            var question = model.GetEntityModel();

            contest.Questions.Add(question);
            this.Data.SaveChanges();

            int savedId = this.Data.Context.Entry(question).Property(pr => pr.Id).CurrentValue;
            model.QuestionId = savedId;

            return this.Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        public JsonResult UpdateQuestionInContest([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var entity = this.GetById(model.QuestionId) as DatabaseModelType;
            this.BaseUpdate(model.GetEntityModel(entity));
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public JsonResult DeleteQuestionFromContest([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            this.Data.ContestQuestions.Delete(model.QuestionId.Value);
            this.Data.SaveChanges();
            return this.GridOperation(request, model);
        }
    }
}