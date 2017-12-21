namespace OJS.Services.Data.Participants
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantsDataService : IParticipantsDataService
    {
        private readonly IEfGenericRepository<Participant> participants;

        public ParticipantsDataService(IEfGenericRepository<Participant> participants) =>
            this.participants = participants;

        public DateTime? GetOfficialContestEndTimeByUserIdAndContestId(string userId, int contestId) =>
            this.participants
                .All()
                .FirstOrDefault(p => p.UserId == userId && p.ContestId == contestId && p.IsOfficial)?
                .ContestEndTime;

        public void Add(Participant participant)
        {
            this.participants.Add(participant);
            this.participants.SaveChanges();
        }

        public void Update(Participant participant)
        {
            this.participants.Update(participant);
            this.participants.SaveChanges();
        }

        public Participant GetWithContestByContestIdUserIdAndIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.participants
                .All()
                .Include(p => p.Contest)
                .FirstOrDefault(p => p.ContestId == contestId && p.UserId == userId && p.IsOfficial == isOfficial);

        public IQueryable<Participant> GetByIdQuery(int participantId) =>
            this.participants
                .All()
                .Where(x => x.Id == participantId);

        public bool IsOfficial(int participantId) =>
            this.participants
                .All()
                .Where(x => x.Id == participantId)
                .Select(x => x.IsOfficial)
                .FirstOrDefault();

        public void ChangeTimeForActiveInOnlineContestByContestIdAndMinutes(int contestId, int minutes)
        {
            var timeSpan = TimeSpan.FromMinutes(minutes);

            var activeParticipants = this.participants
                .All()
                .Where(p => p.ContestId == contestId &&
                    p.IsOfficial &&
                    p.Contest.Type == ContestType.OnlinePracticalExam &&
                    p.ContestEndTime > DateTime.Now);

            foreach (var participant in activeParticipants)
            {
                participant.ContestEndTime = participant.ContestEndTime + timeSpan;
                this.participants.Update(participant);
            }

            this.participants.SaveChanges();
        }
    }
}