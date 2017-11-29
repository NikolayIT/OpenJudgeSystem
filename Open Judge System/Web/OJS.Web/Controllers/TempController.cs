namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
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

        public ActionResult NormalizeParticipantsWithDuplicatedParticipantScores()
        {
            var result = new StringBuilder("<p>Done! Deleted Participant scores:</p><ol>");

            var problemIds = this.Data.Problems.AllWithDeleted().Select(pr => pr.Id).ToList();
            foreach (var problemId in problemIds)
            {
                var participantScoresRepository = new EfGenericRepository<ParticipantScore>(new OjsDbContext());
                var scoresMarkedForDeletion = new List<ParticipantScore>();

                participantScoresRepository
                    .All()
                    .Where(ps => ps.ProblemId == problemId)
                    .GroupBy(p => new { p.ProblemId, p.ParticipantId })
                    .Where(participantScoreGroup => participantScoreGroup.Count() > 1)
                    .ForEach(participantScoreGroup =>
                    {
                        participantScoreGroup
                            .OrderByDescending(ps => ps.Points)
                            .ThenByDescending(ps => ps.Id)
                            .Skip(1)
                            .ForEach(ps => scoresMarkedForDeletion.Add(ps));
                    });
                        
                if (scoresMarkedForDeletion.Any())
                {
                    foreach (var participantScoreForDeletion in scoresMarkedForDeletion)
                    {
                        participantScoresRepository.Delete(participantScoreForDeletion);
                        result.Append($@"<li>ParticipantScore with
                            ParticipantId: {participantScoreForDeletion.ParticipantId} and
                            ProblemId: {participantScoreForDeletion.ProblemId}</li>");
                    }

                    participantScoresRepository.SaveChanges();
                }
            }

            result.Append("</ol>");
            return this.Content(result.ToString());
        }
    }
}