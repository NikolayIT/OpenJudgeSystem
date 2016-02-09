namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.InputModels.AntiCheat;
    using OJS.Web.Areas.Administration.ViewModels.AntiCheat;
    using OJS.Web.ViewModels.Common;
    using OJS.Workers.Tools.AntiCheat;
    using OJS.Workers.Tools.AntiCheat.Contracts;
    using OJS.Workers.Tools.Similarity;

    public class AntiCheatController : AdministrationBaseController
    {
        private const int MinSubmissionPointsToCheckForSimilarity = 20;

        private readonly IPlagiarismDetectorFactory plagiarismDetectorFactory;
        private readonly ISimilarityFinder similarityFinder;

        public AntiCheatController(
            IOjsData data,
            IPlagiarismDetectorFactory plagiarismDetectorFactory,
            ISimilarityFinder similarityFinder)
            : base(data)
        {
            this.plagiarismDetectorFactory = plagiarismDetectorFactory;
            this.similarityFinder = similarityFinder;
        }

        public ActionResult ByIp() => this.View(this.GetContestsListItems());

        public ActionResult RenderByIpGrid(int id, string excludeIps)
        {
            var participantsByIps = this.Data.Participants
                .All()
                .Where(p => p.ContestId == id && p.IsOfficial)
                .Select(AntiCheatByIpAdministrationViewModel.ViewModel)
                .Where(p => p.DifferentIps.Count() > 1)
                .ToList();

            if (!string.IsNullOrEmpty(excludeIps))
            {
                var withoutExcludeIps = this.Data.Participants.All();

                var ipsToExclude = excludeIps.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var ip in ipsToExclude)
                {
                    withoutExcludeIps = withoutExcludeIps
                        .Where(p =>
                            p.ContestId == id &&
                            p.IsOfficial &&
                            p.Submissions.AsQueryable().Count() > 1 &&
                            p.Submissions.AsQueryable()
                                .Where(s => !s.IsDeleted && s.IpAddress != null)
                                .All(s => s.IpAddress != ip));
                }

                participantsByIps.AddRange(withoutExcludeIps.Select(AntiCheatByIpAdministrationViewModel.ViewModel));
            }

            return this.PartialView("_IpGrid", participantsByIps);
        }

        public ActionResult BySubmissionSimilarity()
        {
            var viewModel = new SubmissionSimilarityFiltersInputModel
            {
                PlagiarismDetectorTypes = DropdownViewModel.GetEnumValues<PlagiarismDetectorType>()
            };

            return this.View(viewModel);
        }

        [HttpPost]
        public ActionResult RenderSubmissionsSimilaritiesGrid(int[] contestIds, PlagiarismDetectorType plagiarismDetectorType)
        {
            var participantsSimilarSubmissionGroups =
                this.GetSimilarSubmissions(contestIds, plagiarismDetectorType)
                    .Select(s => new
                    {
                        s.Id,
                        s.ProblemId,
                        s.ParticipantId,
                        s.Points,
                        s.Content,
                        s.CreatedOn,
                        ParticipantName = s.Participant.User.UserName,
                        ProblemName = s.Problem.Name,
                        TestRunResultTypes = s.TestRuns.OrderBy(t => t.Id).Select(t => t.ResultType)
                    })
                    .GroupBy(s => new { s.ProblemId, s.ParticipantId })
                    .Select(g => g.OrderByDescending(s => s.Points).ThenByDescending(s => s.CreatedOn).FirstOrDefault())
                    .GroupBy(s => new { s.ProblemId, s.Points })
                    .ToList();

            var plagiarismDetector = this.GetPlagiarismDetector(plagiarismDetectorType);

            var similarities = new List<SubmissionSimilarityViewModel>();
            for (var index = 0; index < participantsSimilarSubmissionGroups.Count; index++)
            {
                var groupOfSubmissions = participantsSimilarSubmissionGroups[index].ToList();
                for (var i = 0; i < groupOfSubmissions.Count; i++)
                {
                    for (var j = i + 1; j < groupOfSubmissions.Count; j++)
                    {
                        var result = plagiarismDetector.DetectPlagiarism(
                            groupOfSubmissions[i].Content.Decompress(),
                            groupOfSubmissions[j].Content.Decompress(),
                            new List<IDetectPlagiarismVisitor> { new SortAndTrimLinesVisitor() });

                        var save = true;
                        var firstTestRuns = groupOfSubmissions[i].TestRunResultTypes.ToList();
                        var secondTestRuns = groupOfSubmissions[j].TestRunResultTypes.ToList();
                        for (var k = 0; k < firstTestRuns.Count; k++)
                        {
                            if (firstTestRuns[k] != secondTestRuns[k])
                            {
                                save = false;
                                break;
                            }
                        }

                        if (save && result.SimilarityPercentage != 0)
                        {
                            similarities.Add(new SubmissionSimilarityViewModel
                            {
                                ProblemName = groupOfSubmissions[i].ProblemName,
                                Points = groupOfSubmissions[i].Points,
                                Differences = result.Differences.Count,
                                Percentage = result.SimilarityPercentage,
                                FirstSubmissionId = groupOfSubmissions[i].Id,
                                FirstParticipantName = groupOfSubmissions[i].ParticipantName,
                                FirstSubmissionCreatedOn = groupOfSubmissions[i].CreatedOn,
                                SecondSubmissionId = groupOfSubmissions[j].Id,
                                SecondParticipantName = groupOfSubmissions[j].ParticipantName,
                                SecondSubmissionCreatedOn = groupOfSubmissions[j].CreatedOn,
                            });
                        }
                    }
                }
            }

            return this.PartialView("_SubmissionsGrid", similarities.GroupBy(g => g.ProblemName));
        }

        private IEnumerable<SelectListItem> GetContestsListItems()
        {
            var contests = this.Data.Contests
                .All()
                .OrderByDescending(c => c.CreatedOn)
                .Select(c => new { Text = c.Name, Value = c.Id })
                .ToList()
                .Select(c => new SelectListItem { Text = c.Text, Value = c.Value.ToString() });

            return contests;
        }

        private PlagiarismDetectorCreationContext CreatePlagiarismDetectorCreationContext(PlagiarismDetectorType type)
        {
            var result = new PlagiarismDetectorCreationContext(type, this.similarityFinder);

            switch (type)
            {
                case PlagiarismDetectorType.CSharpCompileDisassemble:
                    result.CompilerPath = Settings.CSharpCompilerPath;
                    result.DisassemblerPath = Settings.DotNetDisassemblerPath;
                    break;
                case PlagiarismDetectorType.JavaCompileDisassemble:
                    result.CompilerPath = Settings.JavaCompilerPath;
                    result.DisassemblerPath = Settings.JavaDisassemblerPath;
                    break;
                case PlagiarismDetectorType.PlainText:
                    break;
            }

            return result;
        }

        private IQueryable<Submission> GetSimilarSubmissions(
            IEnumerable<int> contestIds,
            PlagiarismDetectorType plagiarismDetectorType)
        {
            var orExpressionContestIds = ExpressionBuilder.BuildOrExpression<Submission, int>(
                contestIds,
                s => s.Participant.ContestId);

            var plagiarismDetectorTypeCompatibleCompilerTypes = plagiarismDetectorType.GetCompatibleCompilerTypes();
            var orExpressionCompilerTypes = ExpressionBuilder.BuildOrExpression<Submission, CompilerType>(
                plagiarismDetectorTypeCompatibleCompilerTypes,
                s => s.SubmissionType.CompilerType);

            var result = this.Data.Submissions
                .All()
                .Where(orExpressionContestIds)
                .Where(orExpressionCompilerTypes)
                .Where(s => s.Participant.IsOfficial && s.Points >= MinSubmissionPointsToCheckForSimilarity);

            return result;
        }

        private IPlagiarismDetector GetPlagiarismDetector(PlagiarismDetectorType type)
        {
            var plagiarismDetectorCreationContext =
                this.CreatePlagiarismDetectorCreationContext(type);
            var plagiarismDetector =
                this.plagiarismDetectorFactory.CreatePlagiarismDetector(plagiarismDetectorCreationContext);

            return plagiarismDetector;
        }
    }
}