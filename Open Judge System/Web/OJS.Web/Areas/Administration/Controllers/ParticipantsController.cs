namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Newtonsoft.Json;

    using OJS.Data;
    using OJS.Web.Areas.Administration.ViewModels.Participant;
    using OJS.Web.Controllers;

    using AnswerViewModelType = OJS.Web.Areas.Administration.ViewModels.Participant.ParticipantAnswerViewModel;
    using DatabaseModelType = OJS.Data.Models.Participant;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.Participant.ParticipantAdministrationViewModel;

    public class ParticipantsController : KendoGridAdministrationController
    {
        public ParticipantsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Participants
                .All()
                .Select(ViewModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.Participants
                .All()
                .FirstOrDefault(o => o.Id == (int)id);
        }

        public override string GetEntityKeyName()
        {
            return this.GetEntityKeyNameByType(typeof(DatabaseModelType));
        }

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult Contest(int id)
        {
            return this.View(id);
        }

        [HttpPost]
        public ActionResult ReadParticipants([DataSourceRequest]DataSourceRequest request, int? id)
        {
            if (id == null)
            {
                return this.Read(request);
            }

            var participants = this.Data.Participants
                .All()
                .Where(p => p.ContestId == id)
                .Select(ViewModelType.ViewModel);

            var serializationSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var json = JsonConvert.SerializeObject(participants.ToDataSourceResult(request), Formatting.None, serializationSettings);
            return this.Content(json, "application/json");
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var contest = this.Data.Contests.All().FirstOrDefault(c => c.Id == model.ContestId);
            var user = this.Data.Users.All().FirstOrDefault(u => u.Id == model.UserId);

            if (contest == null || user == null)
            {
                if (contest == null)
                {
                    ModelState.AddModelError("ContestId", "Невалидно състезание");
                }

                if (user == null)
                {
                    ModelState.AddModelError("UserId", "Невалиден потребител");
                }

                return this.GridOperation(request, model);
            }

            var participant = model.GetEntityModel();
            participant.Contest = contest;
            participant.User = user;

            model.Id = (int)this.BaseCreate(participant);
            model.UserName = user.UserName;
            model.ContestName = contest.Name;
            this.UpdateAuditInfoValues(model, participant);

            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            this.BaseDestroy(model.Id);
            return this.GridOperation(request, model);
        }

        public JsonResult Contests(string text)
        {
           var contests = this.Data.Contests
                .All()
                .Select(ContestViewModel.ViewModel);

            if (!string.IsNullOrEmpty(text))
            {
                contests = contests.Where(c => c.Name.ToLower().Contains(text.ToLower()));
            }

            return this.Json(contests, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Users(string text)
        {
            var users = this.Data.Users
                .All()
                .Select(UserViewModel.ViewModel);

            if (!string.IsNullOrEmpty(text))
            {
                users = users.Where(c => c.Name.ToLower().Contains(text.ToLower()));
            }

            return this.Json(users, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RenderGrid(int? id)
        {
            return this.PartialView("_Participants", id);
        }

        [HttpGet]
        public FileResult ExportToExcelByContest(DataSourceRequest request, int contestId)
        {
            var data = ((IEnumerable<ViewModelType>)this.GetData()).Where(p => p.ContestId == contestId);
            return this.ExportToExcel(request, data);
        }

        [HttpPost]
        public JsonResult Answers([DataSourceRequest]DataSourceRequest request, int id)
        {
            var answers = this.Data.Participants
                .GetById(id)
                .Answers
                .AsQueryable()
                .Select(AnswerViewModelType.ViewModel);

            return this.Json(answers.ToDataSourceResult(request));
        }

        public JsonResult UpdateParticipantAnswer([DataSourceRequest]DataSourceRequest request, AnswerViewModelType model)
        {
            var participantAnswer = this.Data.Participants
                .GetById(model.ParticipantId)
                .Answers
                .First(a => a.ContestQuestionId == model.ContestQuestionId);

            participantAnswer.Answer = model.Answer;
            participantAnswer.Participant = this.Data.Participants.GetById(model.ParticipantId);
            participantAnswer.ContestQuestion = this.Data.ContestQuestions.GetById(model.ContestQuestionId);
            this.Data.SaveChanges();

            return this.GridOperation(request, model);
        }
    }
}