namespace OJS.Services.Business.Participants
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.SqlServer;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;

    public class ParticipantsBusinessService : IParticipantsBusinessService
    {
        private const string ContestDoesNotExistErrorMessage = "Contest does not exist!";
        private const string ContestDoesNotHaveDurationErrorMessage = "The contest does not have duration set!";
        private readonly IParticipantsDataService participantsData;
        private readonly IContestsDataService contestsData;

        public ParticipantsBusinessService(
            IParticipantsDataService participantsData,
            IContestsDataService contestsData)
        {
            this.participantsData = participantsData;
            this.contestsData = contestsData;
        }

        public Participant CreateNewByContestByUserByIsOfficialAndIsAdmin(
            Contest contest,
            string userId,
            bool isOfficial,
            bool isAdmin)
        {
            var participant = new Participant(contest.Id, userId, isOfficial);

            if (contest.IsOnline && isOfficial)
            {
                participant.ParticipationStartTime = DateTime.Now;
                participant.ParticipationEndTime = DateTime.Now + contest.Duration;

                if (!isAdmin &&
                    !this.contestsData.IsUserLecturerInByContestAndUser(contest.Id, userId))
                {
                    this.AssignRandomProblemsToParticipant(participant, contest);
                }
            }

            this.participantsData.Add(participant);
            return participant;
        }

        public ServiceResult<string> UpdateParticipationEndTimeByIdAndTimeInMinutes(int id, int minutes)
        {
            var participant = this.participantsData.GetById(id);
            if (participant == null)
            {
                throw new ArgumentException("Participant does not exist!");
            }

            if (!participant.Contest.Duration.HasValue)
            {
                return new ServiceResult<string>(ContestDoesNotHaveDurationErrorMessage);
            }

            if (!participant.ParticipationEndTime.HasValue ||
                !participant.ParticipationStartTime.HasValue)
            {
                throw new ArgumentException("Participant does not have compete time set!");
            }

            var newEndTime = participant.ParticipationEndTime.Value.AddMinutes(minutes);
            var minAllowedEndTime = participant.ParticipationStartTime.Value
                .AddMinutes(participant.Contest.Duration.Value.TotalMinutes);

            if (newEndTime < minAllowedEndTime)
            {
                return new ServiceResult<string>("Participant time would be reduced below the contest duration!");
            }

            participant.ParticipationEndTime = newEndTime;

            this.participantsData.Update(participant);

            return ServiceResult<string>.Success(participant.User.UserName);
        }

        public ServiceResult<ICollection<string>> UpdateParticipationsEndTimeByContestByParticipationStartTimeRangeAndTimeInMinutes(
            int contestId,
            int timeInMinutes,
            DateTime participationStartTimeRangeStart,
            DateTime participationStartTimeRangeEnd)
        {
            const string minute = "minute";

            var contest = this.contestsData.GetById(contestId);

            if (contest == null)
            {
                return new ServiceResult<ICollection<string>>(ContestDoesNotExistErrorMessage);
            }

            if (!contest.Duration.HasValue)
            {
                return new ServiceResult<ICollection<string>>(ContestDoesNotHaveDurationErrorMessage);
            }

            var contestTotalDurationInMinutes = contest.Duration.Value.TotalMinutes;

            var invalidForUpdateParticipantUsernames =
                this.participantsData
                    .GetAllOfficialInOnlineContestByContestAndParticipationStartTimeRange(
                        contestId,
                        participationStartTimeRangeStart,
                        participationStartTimeRangeEnd)
                    .Where(p =>
                        SqlFunctions.DateAdd(minute, timeInMinutes, p.ParticipationEndTime) <
                        SqlFunctions.DateAdd(minute, contestTotalDurationInMinutes, p.ParticipationStartTime))
                    .Select(p => p.User.UserName)
                    .ToList();

            var participantsInTimeRange =
                this.participantsData.GetAllOfficialInOnlineContestByContestAndParticipationStartTimeRange(
                    contestId,
                    participationStartTimeRangeStart,
                    participationStartTimeRangeEnd);

            this.participantsData.Update(
                participantsInTimeRange
                    .Where(p =>
                        SqlFunctions.DateAdd(minute, timeInMinutes, p.ParticipationEndTime) >=
                        SqlFunctions.DateAdd(minute, contestTotalDurationInMinutes, p.ParticipationStartTime)),
                p => new Participant
                {
                    ParticipationEndTime = SqlFunctions.DateAdd(
                        minute,
                        timeInMinutes,
                        p.ParticipationEndTime)
                });

            return ServiceResult<ICollection<string>>.Success(invalidForUpdateParticipantUsernames);
        }

        private void AssignRandomProblemsToParticipant(Participant participant, Contest contest)
        {
            var random = new Random();

            var problemGroups = contest.ProblemGroups
                .Where(pg => !pg.IsDeleted && pg.Problems.Any(p => !p.IsDeleted));

            foreach (var problemGroup in problemGroups)
            {
                var problemsInGroup = problemGroup.Problems.Where(p => !p.IsDeleted).ToList();
                if (problemsInGroup.Any())
                {
                    var randomProblem = problemsInGroup[random.Next(0, problemsInGroup.Count)];
                    participant.Problems.Add(randomProblem);
                }
            }
        }
    }
}