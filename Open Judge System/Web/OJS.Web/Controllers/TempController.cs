namespace OJS.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using EntityFramework.Extensions;

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
        private const string CleanSubmissionsForProcessingTableCronExpression = "0 0 * * *";
        private const string DeleteLeftOverFoldersInTempFolderCronExpression = "0 1 * * *";

        private readonly IHangfireBackgroundJobService backgroundJobs;

        public TempController(
            IOjsData data,
            IHangfireBackgroundJobService backgroundJobs)
            : base(data) =>
                this.backgroundJobs = backgroundJobs;

        public ActionResult RegisterJobForCleaningSubmissionsForProcessingTable()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<ISubmissionsForProcessingDataService>(
                "CleanSubmissionsForProcessingTable",
                s => s.Clean(),
                CleanSubmissionsForProcessingTableCronExpression);

            return null;
        }

        public ActionResult RegisterJobForDeletingLeftOverFilesInTempFolder()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob(
                "DeleteLeftOverFoldersInTempFolder",
                () => DirectoryHelpers.DeleteExecutionStrategyWorkingDirectories(),
                DeleteLeftOverFoldersInTempFolderCronExpression);

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

        public ActionResult MigrateProblemsToIndividualGroups()
        {
            var contests = this.Data.Contests
                .AllWithDeleted()
                .Include(c => c.Problems)
                .Where(c => c.NumberOfProblemGroups <= 1 && !c.ProblemGroups.Any())
                .ToList();

            var output = new StringBuilder();
            output.Append($"Done! contests affected: {contests.Count} <ol>");

            foreach (var contest in contests)
            {
                foreach (var problem in contest.Problems)
                {
                    var problemGroup = new ProblemGroup
                    {
                        OrderBy = problem.OrderBy,
                        CreatedOn = DateTime.Now
                    };

                    problemGroup.Problems.Add(problem);

                    contest.ProblemGroups.Add(problemGroup);
                }

                output.Append($"<li>For Contest with <strong>ID:{contest.Id}</strong>: " +
                    $"Added <strong>{contest.Problems.Count}</strong> Problem Groups</li>");
            }

            this.Data.SaveChanges();

            output.Append("</ol>");
            return this.Content(output.ToString());
        }

        public ActionResult MigrateProblemsToSharedGroups()
        {
            var contests = this.Data.Contests
                .AllWithDeleted()
                .Include(c => c.Problems)
                .Where(c => c.NumberOfProblemGroups > 1 && !c.ProblemGroups.Any())
                .ToList();

            var output = new StringBuilder();
            output.Append($"Done! contests affected: {contests.Count} <ol>");

            foreach (var contest in contests)
            {
                output.Append($"<li>Created Problem Groups for Contest: {contest.Name} (ID:{contest.Id})<ol>");
                for (var i = 0; i < contest.NumberOfProblemGroups; i++)
                {
                    var groupNumber = i + 1;
                    var problems = contest.Problems.Where(p => p.GroupNumber == groupNumber).ToList();

                    contest.ProblemGroups.Add(new ProblemGroup
                    {
                        Problems = problems,
                        OrderBy = i,
                        CreatedOn = DateTime.Now
                    });

                    output.Append($"<li>Group with <strong>{problems.Count}</strong> problems</li>");
                }

                output.Append("</ol></li><br/>");
            }

            this.Data.SaveChanges();

            output.Append("</ol>");
            return this.Content(output.ToString());
        }

        public ActionResult MigrateContestStartEndTimeOfParticipantsInParticipationStartEndTime()
        {
            var affected = this.Data.Participants
                .All()
                .Where(p => p.ContestStartTime != null || p.ContestEndTime != null)
                .Update(p => new Participant
                {
                    ParticipationStartTime = p.ContestStartTime,
                    ParticipationEndTime = p.ContestEndTime,
                    ContestStartTime = null,
                    ContestEndTime = null
                });

            return this.Content($"Done! Participants affected: {affected}");
        }

        public ActionResult MakeOrderByZeroToAllContestsInProgrammingBasicsExamsCategory()
        {
            const int ProgrammingBasicsExamsCategoryId = 38;

            var categoryName = this.Data.ContestCategories
                .All()
                .Where(cc => cc.Id == ProgrammingBasicsExamsCategoryId)
                .Select(cc => cc.Name)
                .FirstOrDefault();

            if (categoryName == null)
            {
                return this.Content("Category not found");
            }

            var contestsAffected = this.Data.Contests
                .All()
                .Where(c => c.CategoryId == ProgrammingBasicsExamsCategoryId)
                .Update(c => new Contest { OrderBy = 0 });

            return this.Content($"Changed OrderBy of {contestsAffected} contests in the category {categoryName}");
        }
    }
}