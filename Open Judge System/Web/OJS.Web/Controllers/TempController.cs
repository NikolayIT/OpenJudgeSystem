namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using MissingFeatures;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Web.Common.Attributes;

    [AuthorizeRoles(SystemRole.Administrator)]
    public class TempController : BaseController
    {
        private readonly IHangfireBackgroundJobService backgroundJobs;

        public TempController(IOjsData data, IHangfireBackgroundJobService backgroundJobs)
            : base(data)
        {
            this.backgroundJobs = backgroundJobs;
        }

        public ActionResult RegisterJobForCleaningSubmissionsForProcessingTable()
        {
            string cron = "0 0 * * *";
            this.backgroundJobs.AddOrUpdateRecurringJob<ISubmissionsForProcessingDataService>(
                "CleanSubmissionsForProcessingTable",
                s => s.Clean(),
                cron);
            
            return null;
        }

        public ActionResult RegisterJobForDeletingLeftOverFilesInTempFolder()
        {
            var cron = "0 1 * * *";
            this.backgroundJobs.AddOrUpdateRecurringJob(
                "DeleteLeftOverFoldersInTempFolder",
                () => DirectoryHelpers.DeleteExecutionStrategyWorkingDirectories(),
                cron);

            return null;
        }

        public ActionResult NormalizeParticipants()
        {
            var problemIds = this.Data.Problems.AllWithDeleted().Select(pr => pr.Id).ToArray();
            foreach (var problemId in problemIds)
            {
                var participantScoresData = new EfGenericRepository<ParticipantScore>(new OjsDbContext());
                var participantScoreByParticipantAndProblemId = participantScoresData.All()
                    .Where(ps => ps.ProblemId == problemId)
                    .GroupBy(p => new { p.ProblemId, p.ParticipantId });

                var scoresMarkedForDeletion = new List<ParticipantScore>();
                foreach (var participantScore in participantScoreByParticipantAndProblemId)
                {
                    if (participantScore.Count() > 1)
                    {
                        participantScore
                            .OrderByDescending(ps => ps.Points)
                            .ThenByDescending(ps => ps.Id)
                            .Skip(1)
                            .ForEach(ps => scoresMarkedForDeletion.Add(ps));
                    }
                }

                if (scoresMarkedForDeletion.Any())
                {
                    foreach (var participantScoreForDeletion in scoresMarkedForDeletion)
                    {
                        participantScoresData.Delete(participantScoreForDeletion);
                    }

                    participantScoresData.SaveChanges();
                }
            }

            return null;
        }
    }
}