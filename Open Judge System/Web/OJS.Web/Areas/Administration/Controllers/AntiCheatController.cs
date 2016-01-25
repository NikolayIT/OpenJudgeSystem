namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.AntiCheat;
    using OJS.Workers.Tools.AntiCheat;

    public class AntiCheatController : AdministrationBaseController
    {
        private readonly IPlagiarismDetector plagiarismDetector;

        public AntiCheatController(IOjsData data, IPlagiarismDetector detector)
            : base(data)
        {
            this.plagiarismDetector = detector;
        }

        public ActionResult ByIp()
        {
            return this.View(this.GetContestsListItems());
        }

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
                    withoutExcludeIps = withoutExcludeIps.Where(
                        p => p.ContestId == id
                        && p.IsOfficial
                        && p.Submissions.AsQueryable().Count() > 1
                        && p.Submissions.AsQueryable()
                            .Where(s => !s.IsDeleted && s.IpAddress != null)
                            .All(s => s.IpAddress != ip));
                }

                participantsByIps.AddRange(withoutExcludeIps.Select(AntiCheatByIpAdministrationViewModel.ViewModel));
            }

            return this.PartialView("_IpGrid", participantsByIps);
        }

        public ActionResult BySubmissionSimilarity()
        {
            return this.View(this.GetContestsListItems());
        }

        [HttpPost]
        public ActionResult RenderSubmissionsSimilaritiesGrid(int[] ids)
        {
            var orExpressionIds = ExpressionBuilder.BuildOrExpression<Submission, int>(
                ids,
                s => s.Participant.ContestId);

            var participantsSimilarSubmissionGroups = this.Data.Submissions
                .All()
                .Where(orExpressionIds)
                .Where(s => s.Participant.IsOfficial && s.Points >= 20)
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
                    TestRuns = s.TestRuns.OrderBy(t => t.Id).Select(t => new { t.Id, t.ResultType })
                })
                .GroupBy(s => new { s.ProblemId, s.ParticipantId })
                .Select(g => g.OrderByDescending(s => s.Points).ThenByDescending(s => s.CreatedOn).FirstOrDefault())
                .GroupBy(s => new { s.ProblemId, s.Points })
                .ToList();

            var similarities = new List<SubmissionSimilarityViewModel>();
            foreach (var groupOfSubmissions in participantsSimilarSubmissionGroups)
            {
                var groupAsList = groupOfSubmissions.ToList();
                for (int i = 0; i < groupAsList.Count; i++)
                {
                    for (int j = i + 1; j < groupAsList.Count; j++)
                    {
                        var result = this.plagiarismDetector.DetectPlagiarism(
                            groupAsList[i].Content.Decompress(),
                            groupAsList[j].Content.Decompress(),
                            new List<IDetectPlagiarismVisitor> { new SortAndTrimLinesVisitor() });

                        bool save = true;
                        var firstTestRuns = groupAsList[i].TestRuns.ToList();
                        var secondTestRuns = groupAsList[j].TestRuns.ToList();
                        for (int k = 0; k < firstTestRuns.Count; k++)
                        {
                            if (firstTestRuns[k].ResultType != secondTestRuns[k].ResultType)
                            {
                                save = false;
                                break;
                            }
                        }

                        if (result.SimilarityPercentage != 0 && save)
                        {
                            similarities.Add(new SubmissionSimilarityViewModel
                            {
                                ProblemId = groupAsList[i].ProblemId.Value,
                                ProblemName = groupAsList[i].ProblemName,
                                Points = groupAsList[i].Points,
                                Differences = result.Differences.Count(),
                                Percentage = result.SimilarityPercentage,
                                FirstSubmissionId = groupAsList[i].Id,
                                FirstParticipantName = groupAsList[i].ParticipantName,
                                FirstSubmissionCreatedOn = groupAsList[i].CreatedOn,
                                SecondSubmissionId = groupAsList[j].Id,
                                SecondParticipantName = groupAsList[j].ParticipantName,
                                SecondSubmissionCreatedOn = groupAsList[j].CreatedOn,
                            });
                        }
                    }
                }
            }

            return this.PartialView("_SubmissionsGrid", similarities.GroupBy(g => g.ProblemId));
        }

        private IEnumerable<SelectListItem> GetContestsListItems()
        {
            var contests = this.Data.Contests
                .All()
                .OrderByDescending(c => c.CreatedOn)
                .Select(c =>
                    new
                    {
                        Text = c.Name,
                        Value = c.Id
                    })
                .ToList()
                .Select(c =>
                    new SelectListItem
                    {
                        Text = c.Text,
                        Value = c.Value.ToString()
                    });

            return contests;
        }
    }
}