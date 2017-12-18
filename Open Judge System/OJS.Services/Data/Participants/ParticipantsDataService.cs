namespace OJS.Services.Data.Participants
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantsDataService : IParticipantsDataService
    {
        private readonly IEfGenericRepository<Participant> participants;

        public ParticipantsDataService(IEfGenericRepository<Participant> participants) =>
            this.participants = participants;

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

        public void ChangeTimeForActiveByContestIdAndMinutes(int contestId, int minutes)
        {
            var timeSpan = new TimeSpan(0, 0, minutes, 0);

            var activeParticipants = this.participants
                .All()
                .Where(p => p.ContestId == contestId &&
                    p.IsOfficial &&
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