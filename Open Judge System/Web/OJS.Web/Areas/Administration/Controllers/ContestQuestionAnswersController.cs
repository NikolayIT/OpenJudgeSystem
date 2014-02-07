namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;
    using Kendo.Mvc.Extensions;

    using OJS.Data;
    using OJS.Web.Controllers;

    using DatabaseModelType = OJS.Data.Models.ContestQuestionAnswer;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ContestQuestionAnswer.ContestQuestionAnswerViewModel;

    public class ContestQuestionAnswersController : KendoGridAdministrationController
    {
        private int questionId;

        public ContestQuestionAnswersController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            var answers = this.Data.ContestQuestionAnswers
                .All()
                .Where(q => q.QuestionId == questionId)
                .Select(ViewModelType.ViewModel);

            return answers;
        }

        public override object GetById(object id)
        {
            var answer = this.Data.ContestQuestionAnswers
                .All()
                .FirstOrDefault(q => q.Id == (int)id);

            return answer;
        }

        [HttpPost]
        public JsonResult AnswersInQuestion([DataSourceRequest]DataSourceRequest request, int id)
        {
            this.questionId = id;
            var answers = this.GetData();

            return this.Json(answers.ToDataSourceResult(request));
        }

        [HttpPost]
        public JsonResult AddAnswerToQuestion([DataSourceRequest]DataSourceRequest request, ViewModelType model, int id)
        {
            var question = this.Data.ContestQuestions.All().FirstOrDefault(q => q.Id == id);
            var answer = model.GetEntityModel();

            question.Answers.Add(answer);
            this.Data.SaveChanges();

            int savedId = this.Data.Context.Entry(answer).Property(pr => pr.Id).CurrentValue;
            model.AnswerId = savedId;

            return this.Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        public JsonResult UpdateQuestionInContest([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var entity = this.GetById(model.AnswerId) as DatabaseModelType;
            this.BaseUpdate(model.GetEntityModel(entity));
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public JsonResult DeleteQuestionFromContest([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            this.Data.ContestQuestions.Delete(model.AnswerId.Value);
            this.Data.SaveChanges();
            return this.GridOperation(request, model);
        }
    }
}