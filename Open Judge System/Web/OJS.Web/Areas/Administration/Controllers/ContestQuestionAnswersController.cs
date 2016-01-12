namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Controllers;

    using DatabaseModelType = OJS.Data.Models.ContestQuestionAnswer;
    using Resource = Resources.Areas.Administration.Contests.ContestsControllers;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ContestQuestionAnswer.ContestQuestionAnswerViewModel;

    public class ContestQuestionAnswersController : AdministrationBaseGridController
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
                .Where(q => q.QuestionId == this.questionId)
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

            this.UpdateViewModelValues(model, answer);
            model.QuestionId = question.Id;
            model.QuestionText = question.Text;

            return this.Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        public JsonResult UpdateAnswerInQuestion([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var entity = this.GetById(model.AnswerId) as DatabaseModelType;
            this.BaseUpdate(model.GetEntityModel(entity));
            this.UpdateAuditInfoValues(model, entity);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public JsonResult DeleteAnswerFromQuestion([DataSourceRequest]DataSourceRequest request, ViewModelType model, int id)
        {
            this.Data.ContestQuestionAnswers.Delete(model.AnswerId.Value);
            this.Data.SaveChanges();
            return this.GridOperation(request, model);
        }

        public void DeleteAllAnswers(int id)
        {
            var question = this.Data.ContestQuestions.GetById(id);

            if (question == null)
            {
                throw new ArgumentException(Resource.No_question_by_id, nameof(id));
            }

            question.Answers.Select(a => a.Id).ToList().Each(a => this.Data.ContestQuestionAnswers.Delete(a));
            this.Data.SaveChanges();
        }

        protected void UpdateViewModelValues(ViewModelType viewModel, DatabaseModelType databaseModel)
        {
            var entry = this.Data.Context.Entry(databaseModel);
            viewModel.AnswerId = entry.Property(pr => pr.Id).CurrentValue;
            this.UpdateAuditInfoValues(viewModel, databaseModel);
        }
    }
}