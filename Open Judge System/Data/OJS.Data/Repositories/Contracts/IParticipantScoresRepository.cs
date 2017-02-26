namespace OJS.Data.Repositories.Contracts
{
    using Data.Contracts;
    using OJS.Data.Models;

    public interface IParticipantScoresRepository : IRepository<ParticipantScore>
    {
        void SaveParticipantScore(Submission submission);
    }
}
