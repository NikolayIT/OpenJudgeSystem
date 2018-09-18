namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;
    using Newtonsoft.Json;

    using OJS.Common;
    using OJS.Data;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Administration.Controllers.Common;

    using AnswerViewModelType = OJS.Web.Areas.Administration.ViewModels.Participant.ParticipantAnswerViewModel;
    using DatabaseModelType = OJS.Data.Models.Participant;
    using GlobalResource = Resources.Areas.Administration.Problems.ProblemsControllers;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.Participant.ParticipantAdministrationViewModel;

    public class ParticipantsController : LecturerBaseGridController
    {
        private readonly IContestsDataService contestsData;
        private readonly IUsersDataService usersData;
        private readonly IParticipantsDataService participantsData;

        public ParticipantsController(
            IOjsData data,
            IContestsDataService contestsData,
            IUsersDataService usersData,
            IParticipantsDataService participantsData)
            : base(data)
        {
            this.contestsData = contestsData;
            this.usersData = usersData;
            this.participantsData = participantsData;
        }

        public override IEnumerable GetData() =>
            this.participantsData
                .GetAll()
                .Select(ViewModelType.ViewModel);

        public override object GetById(object id) =>
            this.participantsData
                .GetByIdQuery((int)id)
                .AsNoTracking()
                .FirstOrDefault();

        public override string GetEntityKeyName() =>
            this.GetEntityKeyNameByType(typeof(DatabaseModelType));

        public ActionResult Index() => this.View();

        public ActionResult Contest(int id)
        {
            if (!this.CheckIfUserHasContestPermissions(id))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            return this.View(id);
        }

        [HttpPost]
        public ActionResult ReadParticipants([DataSourceRequest]DataSourceRequest request, int? id)
        {
            if (!id.HasValue)
            {
                return this.Read(request);
            }

            if (!this.CheckIfUserHasContestPermissions(id.Value))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var participants = this.participantsData
                .GetAllByContest(id.Value)
                .Select(ViewModelType.ViewModel);

            var serializationSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var json = JsonConvert.SerializeObject(participants.ToDataSourceResult(request), Formatting.None, serializationSettings);
            return this.Content(json, GlobalConstants.JsonMimeType);
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            if (!this.CheckIfUserHasContestPermissions(model.ContestId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var contest = this.contestsData.GetById(model.ContestId);
            var user = this.usersData.GetById(model.UserId);

            if (contest == null || user == null)
            {
                if (contest == null)
                {
                    this.ModelState.AddModelError("ContestId", GlobalResource.Invalid_contest);
                }

                if (user == null)
                {
                    this.ModelState.AddModelError("UserId", GlobalResource.Invalid_user);
                }

                return this.GridOperation(request, model);
            }

            var participant = model.GetEntityModel();
            participant.ContestId = contest.Id;
            participant.UserId = user.Id;

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

        public ActionResult RenderGrid(int? id) => this.PartialView("_Participants", id);

        [HttpGet]
        public FileResult ExportToExcelByContest(DataSourceRequest request, int contestId)
        {
            if (!this.CheckIfUserHasContestPermissions(contestId))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                throw new UnauthorizedAccessException("No premissions");
            }

            var data = ((IEnumerable<ViewModelType>)this.GetData()).Where(p => p.ContestId == contestId);
            return this.ExportToExcel(request, data);
        }

        [HttpPost]
        public JsonResult Answers([DataSourceRequest]DataSourceRequest request, int id)
        {
            var answers = this.participantsData
                .GetById(id)
                .Answers
                .AsQueryable()
                .Select(AnswerViewModelType.ViewModel);

            return this.Json(answers.ToDataSourceResult(request));
        }

        public JsonResult UpdateParticipantAnswer(
            [DataSourceRequest]DataSourceRequest request,
            AnswerViewModelType model)
        {
            var participant = this.participantsData.GetById(model.ParticipantId);

            var participantAnswer = participant.Answers
                .First(a => a.ContestQuestionId == model.ContestQuestionId);

            participantAnswer.Answer = model.Answer;
            participantAnswer.ParticipantId = participant.Id;
            participantAnswer.ContestQuestion = this.Data.ContestQuestions.GetById(model.ContestQuestionId);
            this.Data.SaveChanges();

            return this.GridOperation(request, model);
        }
    }
}