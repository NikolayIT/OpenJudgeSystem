namespace OJS.Services.Business.Participants
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Data.Participants;

    public class ParticipantsBusinessService : IParticipantsBusinessService
    {
        private readonly IParticipantsDataService participantsData;

        public ParticipantsBusinessService(
            IParticipantsDataService participantsData) =>
                this.participantsData = participantsData;

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

        public Participant CreateNewByContestUserIdAndIsOfficial(Contest contest, string userId, bool isOfficial)
        {
            Participant participant;
            if (contest.IsOnline)
            {
                participant = new Participant(contest.Id, userId, isOfficial)
                {
                    ContestStartTime = DateTime.Now,
                    ContestEndTime = DateTime.Now + contest.Duration
                };

                if (isOfficial)
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