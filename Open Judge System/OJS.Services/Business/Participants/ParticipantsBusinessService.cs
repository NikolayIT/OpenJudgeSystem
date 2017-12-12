namespace OJS.Services.Business.Participants
{
    using System;
    using System.Linq;
    using OJS.Common.Models;
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
            if (contest.Type == ContestType.OnlinePractialExam &&
                contest.Participants.Any(p => p.UserId == userId && p.IsOfficial))
            {
                var contestEndTime = this.participantsData.GetOfficialContestEndTimeByUserIdAndContestId(
                    userId,
                    contest.Id);

                return contestEndTime.HasValue && contestEndTime >= DateTime.Now;
            }

            return contest.CanBeCompeted;
        }

        public Participant CreateNewByContestUserIdAndIsOfficial(Contest contest, string userId, bool isOfficial)
        {
            Participant participant;
            if (contest.Type == ContestType.OnlinePractialExam)
            {
                participant = new Participant(contest.Id, userId, isOfficial)
                {
                    ContestStartTime = DateTime.Now,
                    ContestEndTime = DateTime.Now + contest.Duration
                };
            }
            else
            {
                participant = new Participant(contest.Id, userId, isOfficial);
            }

            this.participantsData.Add(participant);
            return participant;
        }
    }
}