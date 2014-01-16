namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Newtonsoft.Json;

    using OJS.Data;
    using OJS.Web.Areas.Administration.ViewModels.Participant;
    using OJS.Web.Controllers;

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
            var participant = model.GetEntityModel();
            var contest = this.Data.Contests.All().FirstOrDefault(c => c.Id == model.ContestId);
            var user = this.Data.Users.All().FirstOrDefault(u => u.Id == model.UserId);
            participant.Contest = contest;
            participant.User = user;
            this.BaseCreate(participant);

            model.UserName = user.UserName;
            model.ContestName = contest.Name;
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

        public ActionResult RenderGrid(int id)
        {
            return this.PartialView("_Participants", id);
        }
    }
}