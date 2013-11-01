namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ParticipantResultsViewModel
    {
        public static Expression<Func<Participant, ParticipantResultsViewModel>> FromParticipant
        {
            get
            {
                return participant => new ParticipantResultsViewModel
                {
                    ParticipantName = participant.User.UserName,
                    ProblemResults = participant.Submissions
                                                .AsQueryable()
                                                .GroupBy(x => x.Problem)
                                                .Select(x => new ProblemResultPairViewModel
                                                {
                                                    Id = x.Key.Id,
                                                    Result = (int)Math.Round(x.Max(runs => runs.TestRuns.Count(test => test.ResultType == TestRunResultType.CorrectAnswer)) * x.Key.MaximumPoints / (float)x.Key.Tests.Count)
                                                })
                };
            }
        }

        public string ParticipantName { get; set; }

        public IEnumerable<ProblemResultPairViewModel> ProblemResults { get; set; }

        public int Total
        {
            get
            {
                return this.ProblemResults.Sum(x => x.Result);
            }
        }
    }
}