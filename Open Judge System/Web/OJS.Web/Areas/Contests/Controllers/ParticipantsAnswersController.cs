namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Areas.Contests.ViewModels.ParticipantsAnswers;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Controllers;

    public class ParticipantsAnswersController : KendoGridAdministrationController
    {
        private int contestId;

        public ParticipantsAnswersController(IOjsData data)
            : base(data)
        {
        }

        [OverrideAuthorize(Roles = "KidsTeacher, Administrator")]
        public ActionResult Details(int id)
        {
            this.contestId = id;

            return this.View(id);
        }

        [OverrideAuthorize(Roles = "KidsTeacher, Administrator")]
        public override object GetById(object id)
        {
            return this.Data.Contests.All().FirstOrDefault(x => x.Id == (int)id);
        }

        [HttpPost]
        [OverrideAuthorize(Roles = "KidsTeacher, Administrator")]
        public ActionResult ReadData([DataSourceRequest] DataSourceRequest request, int contestId)
        {
            this.contestId = contestId;

            return this.Read(request);
        }

        [HttpGet]
        [OverrideAuthorize(Roles = "KidsTeacher, Administrator")]
        public FileResult ExcelExport([DataSourceRequest] DataSourceRequest request, int contestId)
        {
            this.contestId = contestId;

            return this.ExportToExcel(request, this.GetData());
        }

        [OverrideAuthorize(Roles = "KidsTeacher, Administrator")]
        public override IEnumerable GetData()
        {
            var result = this.Data
                .Contests
                .All()
                .Where(x => x.Id == this.contestId)
                .SelectMany(
                    x =>
                    x.Participants
                        .Select(z => new ParticipantsAnswersViewModel
                        {
                            Id = z.Id,
                            Points = z.Submissions.Any()
                                ? z.Submissions
                                    .Where(s => !s.Problem.IsDeleted)
                                    .GroupBy(s => s.Problem)
                                    .Select(gr => gr.Any()
                                        ? gr.OrderByDescending(s => s.Points).Select(q => q.Points).Take(1).FirstOrDefault()
                                        : 0)
                                    .Sum(q => q)
                                    : 0,
                            ParticipantUsername = z.User.UserName,
                            ParticipantFullName = z.User.UserSettings.FirstName + " " + z.User.UserSettings.LastName,
                            Answer = z.Answers.Any()
                                ? z.Answers.FirstOrDefault().Answer
                                : string.Empty
                        }))
                .ToList();

            foreach (var item in result)
            {
                int id = 0;

                if (int.TryParse(item.Answer, out id))
                {
                    item.Answer = this.Data.ContestQuestionAnswers.All().FirstOrDefault(x => x.Id == id)?.Text;
                }
            }

            return result
                .OrderByDescending(s => s.Answer)
                .ThenByDescending(s => s.Points);
        }
    }
}
