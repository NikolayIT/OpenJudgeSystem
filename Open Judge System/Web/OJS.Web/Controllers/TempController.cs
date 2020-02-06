namespace OJS.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using EntityFramework.Extensions;
    using Hangfire;
    using MissingFeatures;

    using OJS.Common;
    using OJS.Common.Helpers;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Services.Business.ParticipantScores;
    using OJS.Services.Business.Submissions.ArchivedSubmissions;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Common.HttpRequester;
    using OJS.Services.Common.HttpRequester.Models.Users;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Web.Common.Attributes;
    using OJS.Workers.Common.Helpers;

    [AuthorizeRoles(SystemRole.Administrator)]
    public class TempController : BaseController
    {
        private readonly IHangfireBackgroundJobService backgroundJobs;
        private readonly IProblemGroupsDataService problemGroupsData;
        private readonly IParticipantsDataService participantsData;
        private readonly IHttpRequesterService httpRequester;

        public TempController(
            IOjsData data,
            IHangfireBackgroundJobService backgroundJobs,
            IProblemGroupsDataService problemGroupsData,
            IParticipantsDataService participantsData,
            IHttpRequesterService httpRequester)
            : base(data)
        {
            this.backgroundJobs = backgroundJobs;
            this.problemGroupsData = problemGroupsData;
            this.participantsData = participantsData;
            this.httpRequester = httpRequester;
        }

        public ActionResult RegisterJobForCleaningSubmissionsForProcessingTable()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<ISubmissionsForProcessingDataService>(
                "CleanSubmissionsForProcessingTable",
                s => s.Clean(),
                Cron.Daily());

            return null;
        }

        public ActionResult RegisterJobForDeletingLeftOverFilesInTempFolder()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob(
                "DeleteLeftOverFoldersInTempFolder",
                () => DirectoryHelpers.DeleteExecutionStrategyWorkingDirectories(),
                Cron.Daily(1));

            return null;
        }

        public ActionResult RegisterJobForArchivingOldSubmissions()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<IArchivedSubmissionsBusinessService>(
                "ArchiveOldSubmissions",
                s => s.ArchiveOldSubmissions(null),
                Cron.Weekly(DayOfWeek.Monday, 1, 30));

            return null;
        }

        public ActionResult RegisterJobForNormalizingSubmissionAndParticipantScorePoints()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<IParticipantScoresBusinessService>(
                "NormalizePointsExceedingMaxPointsForProblem",
                x => x.NormalizeAllPointsThatExceedAllowedLimit(),
                Cron.Daily(2));

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

        public ActionResult DeleteEmptyProblemGroups()
        {
            var softDeleted = this.problemGroupsData
                .GetAll()
                .Where(pg => pg.Problems.All(p => p.IsDeleted))
                .Update(pg => new ProblemGroup
                {
                    IsDeleted = true
                });

            var hardDeleted = this.problemGroupsData
                .GetAllWithDeleted()
                .Where(pg => !pg.Problems.Any())
                .Delete();

            return this.Content($"Done! ProblemGroups set to deleted: {softDeleted}" +
                $"<br/> ProblemGroups hard deleted: {hardDeleted}");
        }

        public ActionResult DeleteDuplicatedParticipantsInSameContest()
        {
            var deletedParticipantsCount = this.participantsData
                .GetAll()
                .GroupBy(p => new { p.UserId, p.IsOfficial, p.ContestId })
                .Where(gr => gr.Count() > 1)
                .SelectMany(gr => gr)
                .Where(p => !p.Scores.Any())
                .Delete();

            return this.Content($"Done! Deleted Participants count: {deletedParticipantsCount}");
        }

        public async Task<ActionResult> EqualizeUserIdFromSulsAndJudgeByUserName(string userName)
        {
            var userInfoResponse = await this.httpRequester.GetAsync<ExternalUserInfoModel>(
                new { userName },
                string.Format(UrlConstants.GetUserInfoByUsernameApiFormat, Settings.SulsPlatformBaseUrl),
                Settings.ApiKey);

            if (!userInfoResponse.IsSuccess || userInfoResponse.Data == null)
            {
                return this.Content($"Cannot get user info from SoftUni Platform for user \"{userName}\"");
            }

            var correctUserId = userInfoResponse.Data.Id;

            var user = this.Data.Users.GetByUsername(userName);

            if (user == null)
            {
                return this.Content($"Cannot find user with UserName: \"{userName}\" in the database");
            }

            if (user.Id == correctUserId)
            {
                return this.Content($"User \"{userName}\" has the same Id as in SoftUni Platform");
            }

            var tempUserIdToStoreParticipants = this.Data.Users.GetByUsername("gogo4ds")?.Id;

            if (string.IsNullOrEmpty(tempUserIdToStoreParticipants))
            {
                return this.Content("Invalid temp UserId to store participants");
            }

            var participantsForUser = this.participantsData.GetAllByUser(user.Id);

            var participantIdsForUser = participantsForUser.Select(x => x.Id).ToList();

            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                this.participantsData.Update(
                    participantsForUser,
                    _ => new Participant
                    {
                        UserId = tempUserIdToStoreParticipants,
                    });

                this.Data.Users.Update(
                    u => u.UserName == userName,
                    _ => new UserProfile
                    {
                        Id = correctUserId,
                    });

                if (participantIdsForUser.Any())
                {
                    participantsForUser = this.participantsData
                        .GetAll()
                        .Where(p => participantIdsForUser.Contains(p.Id));

                    this.participantsData.Update(
                        participantsForUser,
                        _ => new Participant
                        {
                            UserId = correctUserId,
                        });
                }

                scope.Complete();
            }

            return this.Content(
                $@"Done. Changed Id of User ""{userName}"" to match the Id from Suls that is ""{correctUserId}"" 
                and modified his {participantIdsForUser.Count} Participants to point to the new Id");
        }
    }
}