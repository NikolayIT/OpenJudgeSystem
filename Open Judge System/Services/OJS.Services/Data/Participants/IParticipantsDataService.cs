namespace OJS.Services.Data.Participants
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsDataService : IService
    {
        Participant GetById(int id);

        Participant GetByContestByUserAndByIsOfficial(int contestId, string userId, bool isOfficial);

        Participant GetWithContestByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial);

        IQueryable<Participant> GetByIdQuery(int id);

        IQueryable<Participant> GetAll();

        IQueryable<Participant> GetAllByUser(string userId);

        IQueryable<Participant> GetAllByContest(int contestId);

        IQueryable<Participant> GetAllOfficialByContest(int contestId);

        IQueryable<Participant> GetAllByContestAndIsOfficial(int contestId, bool isOfficial);

        IQueryable<Participant> GetAllOfficialInOnlineContestByContestAndParticipationStartTimeRange(
            int contestId,
            DateTime participationStartTimeRangeStart,
            DateTime participationStartTimeRangeEnd);

        bool ExistsByIdAndContest(int id, int contestId);

        bool ExistsByContestAndUser(int contestId, string userId);

        bool ExistsByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial);

        bool IsOfficialById(int id);

        void Add(Participant participant);

        void Update(Participant participant);

        void Update(
            IQueryable<Participant> participantsQuery,
            Expression<Func<Participant, Participant>> updateExpression);

        void Delete(IEnumerable<Participant> participants);

        void InvalidateByContestAndIsOfficial(int contestId, bool isOfficial);
    }
}