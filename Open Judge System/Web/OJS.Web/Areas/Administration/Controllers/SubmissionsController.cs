namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Data;
    using OJS.Web.Controllers;

    using DatabaseModelType = OJS.Data.Models.Submission;
    using GridModelType = OJS.Web.Areas.Administration.ViewModels.Submission.SubmissionAdministrationGridViewModel;
    using ModelType = OJS.Web.Areas.Administration.ViewModels.Submission.SubmissionAdministrationViewModel;

    public class SubmissionsController : KendoGridAdministrationController
    {
        private const string SuccessfulCreationMessage = "Решението беше добавено успешно!";
        private const string SuccessfulEditMessage = "Решението беше променено успешно!";
        private const string InvalidSubmissionMessage = "Невалидно решение!";
        private const string RetestSuccessful = "Решението беше успешно пуснато за ретестване!";

        private int? contestId;

        public SubmissionsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            var submissions = this.Data.Submissions.All();

            if (this.contestId != null)
            {
                submissions = submissions.Where(s => s.Problem.ContestId == this.contestId);
            }

            return submissions.Select(GridModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.Submissions
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

        [HttpGet]
        public ActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult ReadSubmissions([DataSourceRequest]DataSourceRequest request, int? id)
        {
            this.contestId = id;
            return this.Read(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ModelType model)
        {
            if (model != null && this.ModelState.IsValid)
            {
                this.BaseCreate(model.GetEntityModel());
                this.TempData[GlobalConstants.InfoMessage] = SuccessfulCreationMessage;
                return this.RedirectToAction(GlobalConstants.Index);
            }

            return this.View(model);
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            var submission = this.Data.Submissions
                .All()
                .Where(subm => subm.Id == id)
                .Select(ModelType.ViewModel)
                .FirstOrDefault();

            if (submission == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = InvalidSubmissionMessage;
                this.RedirectToAction(GlobalConstants.Index);
            }

            return this.View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ModelType model)
        {
            if (model != null && this.ModelState.IsValid)
            {
                var entity = this.GetById(model.Id) as DatabaseModelType;
                this.BaseUpdate(model.GetEntityModel(entity));
                this.TempData[GlobalConstants.InfoMessage] = SuccessfulEditMessage;
                return this.RedirectToAction(GlobalConstants.Index);
            }

            return this.View(model);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var submission = this.Data.Submissions
                .All()
                .Where(subm => subm.Id == id)
                .Select(GridModelType.ViewModel)
                .FirstOrDefault();

            if (submission == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = InvalidSubmissionMessage;
                this.RedirectToAction(GlobalConstants.Index);
            }

            return this.View(submission);
        }

        public ActionResult ConfirmDelete(int id)
        {
            var submission = this.Data.Submissions
                .All()
                .FirstOrDefault(subm => subm.Id == id);

            if (submission == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = InvalidSubmissionMessage;
                this.RedirectToAction(GlobalConstants.Index);
            }

            foreach (var testRun in submission.TestRuns.ToList())
            {
                this.Data.TestRuns.Delete(testRun.Id);
            }

            this.Data.Submissions.Delete(id);
            this.Data.SaveChanges();

            return this.RedirectToAction(GlobalConstants.Index);
        }

        public JsonResult GetSubmissionTypes()
        {
            var dropDownData = this.Data.SubmissionTypes
                .All()
                .ToList()
                .Select(subm => new SelectListItem
                {
                    Text = subm.Name,
                    Value = subm.Id.ToString(),
                });

            return this.Json(dropDownData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Retest(int id)
        {
            var submission = this.Data.Submissions.GetById(id);

            if (submission == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = InvalidSubmissionMessage;
            }
            else
            {
                submission.Processed = false;
                submission.Processing = false;
                this.Data.SaveChanges();

                this.TempData[GlobalConstants.InfoMessage] = RetestSuccessful;
            }

            return this.RedirectToAction("View", "Submissions", new { area = "Contests", id = id });
        }

        public JsonResult GetProblems(string text)
        {
            var dropDownData = this.Data.Problems.All();

            if (!string.IsNullOrEmpty(text))
            {
                dropDownData = dropDownData.Where(pr => pr.Name.ToLower().Contains(text.ToLower()));
            }

            var result = dropDownData
                .ToList()
                .Select(pr => new SelectListItem
                {
                    Text = pr.Name,
                    Value = pr.Id.ToString(),
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetParticipants(string text, int problem)
        {
            var selectedProblem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == problem);

            var dropDownData = this.Data.Participants.All().Where(part => part.ContestId == selectedProblem.ContestId);

            if (!string.IsNullOrEmpty(text))
            {
                dropDownData = dropDownData.Where(part => part.User.UserName.ToLower().Contains(text.ToLower()));
            }

            var result = dropDownData
                .ToList()
                .Select(part => new SelectListItem
                {
                    Text = part.User.UserName,
                    Value = part.Id.ToString(),
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RenderGrid(int? id)
        {
            return this.PartialView("_SubmissionsGrid", id);
        }

        public JsonResult Contests(string text)
        {
            var contests = this.Data.Contests
                .All()
                .Select(c => new 
                    {
                        Id = c.Id,
                        Name = c.Name
                    });

            if (!string.IsNullOrEmpty(text))
            {
                contests = contests.Where(c => c.Name.ToLower().Contains(text.ToLower()));
            }

            return this.Json(contests, JsonRequestBehavior.AllowGet);
        }
    }
}