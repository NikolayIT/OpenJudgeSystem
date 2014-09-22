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
    using OJS.Web.Areas.Administration.ViewModels.AntiCheat;
    using OJS.Web.Controllers;

    public class AntiCheatController : AdministrationController
    {
        public AntiCheatController(IOjsData data)
            : base(data)
        {
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
            var orExpressionIds = ExpressionBuilder.BuildOrExpression<Submission, int>(ids, s => s.Participant.ContestId);
            var participantsSimilarSubmissionGroups = this.Data.Submissions
                .All()
                .Where(orExpressionIds)
                .Where(s => s.Participant.IsOfficial && s.Points != 0)
                .Select(s =>
                    new
                    {
                        s.Id,
                        s.ProblemId,
                        s.ParticipantId,
                        s.Points,
                        s.Content,
                        s.CreatedOn,
                        ParticipantName = s.Participant.User.UserName,
                        ProblemName = s.Problem.Name
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
                        var result = this.plagiarismDetector.Check(groupAsList[i].Content.Decompress(), groupAsList[j].Content.Decompress());
                        similarities.Add(new SubmissionSimilarityViewModel
                        {
                            ProblemId = groupAsList[i].ProblemId.Value,
                            ProblemName = groupAsList[i].ProblemName,
                            Points = groupAsList[i].Points,
                            Differences = result.Count(),
                            FirstSubmissionId = groupAsList[i].Id,
                            FirstParticipantName = groupAsList[i].ParticipantName,
                            FirstSubmissionCreatedOn = groupAsList[i].CreatedOn,
                            SecondSubmissionId = groupAsList[j].Id,
                            SecondParticipantName = groupAsList[j].ParticipantName,
                            SecondSubmissionCreatedOn = groupAsList[j].CreatedOn
                        });
                    }
                }
            }

            return this.PartialView("_SubmissionsGrid", similarities.GroupBy(g => g.ProblemId));
        }

        private PlagiarismDetector plagiarismDetector = new PlagiarismDetector();

        private class PlagiarismResult
        {
            public int StartFirst { get; set; }

            public int StartSecond { get; set; }

            public int DeletedFirst { get; set; }

            public int InsertedSecond { get; set; }
        }

        private class PlagiarismDetector
        {
            public IEnumerable<PlagiarismResult> Check(string first, string second)
            {
                return new List<PlagiarismResult>();
            }
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