﻿namespace OJS.Services.Business.Participants
{
    using System;
    using System.Linq;
    using OJS.Common.Models;
    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;

    public class ParticipantsBusinessService : IParticipantsBusinessService
    {
        private readonly IParticipantsDataService participantsData;
        private readonly IContestsDataService contestsData;

        public ParticipantsBusinessService(
            IParticipantsDataService participantsData,
            IContestsDataService contestsData)
        {
            this.participantsData = participantsData;
            this.contestsData = contestsData;
        }

        public bool CanCompeteByContestAndUserId(Contest contest, string userId)
        {
            if (contest.IsOnline &&
                contest.Participants.Any(p => p.UserId == userId && p.IsOfficial))
            {
                var contestEndTime = this.participantsData.GetOfficialContestEndTimeByUserIdAndContestId(
                    userId,
                    contest.Id);

                return contestEndTime.HasValue && contestEndTime >= DateTime.Now;
            }

            return contest.CanBeCompeted;
        }

        public Participant CreateNewByContestUserIdIsOfficialAndIsAdmin(
            Contest contest,
            string userId,
            bool isOfficial,
            bool isAdmin)
        {
            Participant participant;
            if (contest.IsOnline)
            {
                participant = new Participant(contest.Id, userId, isOfficial)
                {
                    ContestStartTime = DateTime.Now,
                    ContestEndTime = DateTime.Now + contest.Duration
                };

                if (isOfficial &&
                    !isAdmin &&
                    !this.contestsData.IsUserLecturerInByContestIdAndUserId(contest.Id, userId))
                {
                    this.AssignRandomProblemsToParticipant(participant, contest);
                }
            }
            else
            {
                participant = new Participant(contest.Id, userId, isOfficial);
            }

            this.participantsData.Add(participant);
            return participant;
        }

        public void ChangeTimeForActiveInOnlineContestByContestIdAndMinutes(
            int contestId, 
            int minutes, 
            DateTime after,
            DateTime before)
        {
            var timeSpan = TimeSpan.FromMinutes(minutes);

            var activeParticipants = this.participantsData
                .GetOfficialInOnlineContestByContestStartTimeAfterDateTimeAndBeforeDateTimeAndContest(
                    contestId,
                    after,
                    before)
                .ToList();

            foreach (var participant in activeParticipants)
            {
                participant.ContestEndTime = participant.ContestEndTime + timeSpan;
                this.participantsData.Update(participant);
            }
        }

        private void AssignRandomProblemsToParticipant(Participant participant, Contest contest)
        {
            var random = new Random();

            var problemGroups = contest.Problems
                .Where(p => !p.IsDeleted)
                .GroupBy(p => p.GroupNumber)
                .Select(problemGroup => problemGroup.ToList());

            foreach (var problemGroup in problemGroups)
            {
                var randomProblem = problemGroup[random.Next(0, problemGroup.Count)];
                participant.Problems.Add(randomProblem);
            }
        }
    }
}