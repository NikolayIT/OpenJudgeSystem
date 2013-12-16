namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using Ionic.Zip;

    using Kendo.Mvc.UI;

    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Common;
    using OJS.Web.Controllers;

    using ModelType = OJS.Web.Areas.Administration.ViewModels.Contest.ContestAdministrationViewModel;

    public class ContestsController : KendoGridAdministrationController
    {
        private const string NoActiveContests = "Няма активни състезания";
        private const string NoFutureContests = "Няма бъдещи състезания";
        private const string NoLatestContests = "Нямa последни състезaния";

        public ContestsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Contests
                .All()
                .Where(x => !x.IsDeleted)
                .Select(ContestAdministrationViewModel.ViewModel);
        }

        public ActionResult Index()
        {
            this.GenerateCategoryDropDownData();
            return this.View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseCreate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseUpdate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseDestroy(request, model.ToEntity);
        }

        public ZipFileResult Export(int id, bool compete)
        {
            var contest = this.Data.Contests.GetById(id);
            var problems = contest.Problems.OrderBy(x => x.Name).ToList();
            var participants =
                this.Data.Participants.All()
                    .Where(x => x.ContestId == id && x.IsOfficial == compete)
                    .Select(
                        x => new { x.Id, x.User.UserName, x.User.UserSettings.FirstName, x.User.UserSettings.LastName, })
                    .ToList()
                    .OrderBy(x => x.UserName);

            // Prepare file comment
            var fileComment = new StringBuilder();
            fileComment.AppendLine(string.Format("{1} submissions for {0}", contest.Name, compete ? "Contest" : "Practice"));
            fileComment.AppendLine(string.Format("Number of participants: {0}", participants.Count()));
            fileComment.AppendLine();
            fileComment.AppendLine("Problems:");
            foreach (var problem in problems)
            {
                fileComment.AppendLine(
                    string.Format(
                        "{0} - {1} points, time limit: {2:0.000} sec., memory limit: {3:0.00} MB",
                        problem.Name,
                        problem.MaximumPoints,
                        problem.TimeLimit / 1000.0,
                        problem.MemoryLimit / 1024.0 / 1024.0));
            }

            // Prepare zip file
            var file = new ZipFile
                           {
                               Comment = fileComment.ToString(),
                               AlternateEncoding = Encoding.UTF8,
                               AlternateEncodingUsage = ZipOption.AsNecessary
                           };

            // Add participants solutions
            foreach (var participant in participants)
            {
                // Create directory with the participants name
                var directoryName =
                    string.Format("{0} ({1} {2})", participant.UserName, participant.FirstName.Trim(), participant.LastName.Trim())
                        .ToValidFilePath();
                var directory = file.AddDirectoryByName(directoryName);

                foreach (var problem in problems)
                {
                    // Find submission
                    var bestSubmission =
                        this.Data.Submissions.All()
                            .Where(
                                submission =>
                                submission.ParticipantId == participant.Id && submission.ProblemId == problem.Id)
                            .OrderByDescending(submission => submission.Points)
                            .ThenByDescending(submission => submission.Id)
                            .FirstOrDefault();

                    // Create file if submission exists
                    if (bestSubmission != null)
                    {
                        var fileName =
                            string.Format("{0}.{1}", problem.Name, bestSubmission.SubmissionType.FileNameExtension)
                                .ToValidFileName();

                        var entry = file.AddEntry(
                            string.Format("{0}\\{1}", directoryName, fileName),
                            bestSubmission.ContentAsString.ToByteArray());
                        entry.CreationTime = bestSubmission.CreatedOn;
                        entry.ModifiedTime = bestSubmission.CreatedOn;
                    }
                }
            }

            // Send file to the user
            var zipFileName = string.Format("{1} submissions for {0}.zip", contest.Name, compete ? "Contest" : "Practice");
            return new ZipFileResult(file, zipFileName);
        }

        public ActionResult GetFutureContests([DataSourceRequest]DataSourceRequest request)
        {
            var futureContests = this.Data.Contests
                .AllFuture()
                .OrderBy(contest => contest.StartTime)
                .Take(3)
                .Select(ShortContestAdministrationViewModel.FromContest);

            if (!futureContests.Any())
            {
                return this.Content(NoFutureContests);
            }

            return this.PartialView("_QuickContestsGrid", futureContests);
        }

        public ActionResult GetActiveContests([DataSourceRequest]DataSourceRequest request)
        {
            var activeContests = this.Data.Contests
                .AllActive()
                .OrderBy(contest => contest.EndTime)
                .Take(3)
                .Select(ShortContestAdministrationViewModel.FromContest);

            if (!activeContests.Any())
            {
                return this.Content(NoActiveContests);
            }

            return this.PartialView("_QuickContestsGrid", activeContests);
        }

        public ActionResult GetLatestContests([DataSourceRequest]DataSourceRequest request)
        {
            var latestContests = this.Data.Contests
                .AllVisible()
                .OrderByDescending(contest => contest.CreatedOn)
                .Take(3)
                .Select(ShortContestAdministrationViewModel.FromContest);

            if (!latestContests.Any())
            {
                return this.Content(NoLatestContests);
            }

            return this.PartialView("_QuickContestsGrid", latestContests);
        }

        private void GenerateCategoryDropDownData()
        {
            var dropDownData = this.Data.ContestCategories
                .All()
                .ToList()
                .Select(cat => new SelectListItem
                {
                    Text = cat.Name,
                    Value = cat.Id.ToString(CultureInfo.InvariantCulture),
                });

            // TODO: Improve not to use ViewData
            this.ViewData["CategoryIdData"] = dropDownData;
        }
    }
}