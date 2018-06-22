namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Data.Contests;
    using OJS.Web.Areas.Administration.Controllers.Common;

    using DatabaseAnswerModelType = OJS.Data.Models.ContestQuestionAnswer;
    using DatabaseModelType = OJS.Data.Models.ContestQuestion;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ContestQuestion.ContestQuestionViewModel;

    public class ContestQuestionsController : AdministrationBaseGridController
    {
        private readonly IContestsDataService contestsData;
        private int contestId;

        public ContestQuestionsController(
            IOjsData data,
            IContestsDataService contestsData)
            : base(data) =>
            this.contestsData = contestsData;

        public override IEnumerable GetData()
        {
            var questions = this.Data.ContestQuestions
                .All()
                .Where(q => q.ContestId == this.contestId)
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
            var contest = this.contestsData.GetById(id);
            var question = model.GetEntityModel();

            contest.Questions.Add(question);
            this.contestsData.Update(contest);

            this.UpdateAuditInfoValues(model, question);
            model.QuestionId = this.Data.Context.Entry(question).Property(pr => pr.Id).CurrentValue;
            model.ContestId = contest.Id;

            return this.Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        public JsonResult UpdateQuestionInContest([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var entity = this.GetById(model.QuestionId) as DatabaseModelType;
            this.BaseUpdate(model.GetEntityModel(entity));
            this.UpdateAuditInfoValues(model, entity);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public JsonResult DeleteQuestionFromContest([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            this.Data.ContestQuestions.Delete(model.QuestionId.Value);
            this.Data.SaveChanges();
            return this.GridOperation(request, model);
        }

        public ActionResult CopyFromAnotherContest(int id)
        {
            var contests = this.contestsData
                .GetAll()
                .OrderByDescending(c => c.CreatedOn)
                .Select(c => new { Text = c.Name, Value = c.Id });

            this.ViewBag.ContestId = id;

            return this.PartialView("_CopyQuestionsFromContest", contests);
        }

        public void CopyTo(int id, int contestFrom, bool? deleteOld)
        {
            var copyFromContest = this.contestsData.GetById(contestFrom);
            var copyToContest = this.contestsData.GetById(id);

            if (deleteOld.HasValue && deleteOld.Value)
            {
                var oldQuestions = copyToContest.Questions.Select(q => q.Id).ToList();
                this.DeleteQuestions(oldQuestions);
            }

            var questionsToCopy = copyFromContest.Questions.ToList();
            this.CopyQuestionsToContest(copyToContest, questionsToCopy);
        }

        private void DeleteQuestions(IEnumerable<int> questions)
        {
            foreach (var question in questions)
            {
                this.Data.ContestQuestions.Delete(question);
            }

            this.Data.SaveChanges();
        }

        private void CopyQuestionsToContest(Contest contest, IEnumerable<ContestQuestion> questions)
        {
            foreach (var question in questions)
            {
                var newQuestion = new DatabaseModelType
                {
                    Text = question.Text,
                    Type = question.Type,
                    AskOfficialParticipants = question.AskOfficialParticipants,
                    AskPracticeParticipants = question.AskPracticeParticipants,
                    RegularExpressionValidation = question.RegularExpressionValidation
                };

                foreach (var answer in question.Answers)
                {
                    newQuestion.Answers.Add(new DatabaseAnswerModelType { Text = answer.Text });
                }

                contest.Questions.Add(newQuestion);
            }

            this.contestsData.Update(contest);
        }
    }
}