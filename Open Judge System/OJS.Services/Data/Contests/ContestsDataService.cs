﻿namespace OJS.Services.Data.Contests
{
    using System;
    using System.Linq;

    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ContestsDataService : IContestsDataService
    {
        private readonly IEfDeletableEntityRepository<Contest> contests;

        public ContestsDataService(IEfDeletableEntityRepository<Contest> contests) =>
            this.contests = contests;

        public Contest GetById(int contestId) => this.contests.GetById(contestId);

        public IQueryable<Contest> GetByIdQuery(int contestId) =>
            this.contests.All().Where(c => c.Id == contestId);

        public IQueryable<Contest> GetAllActive() =>
            this.contests
                .All()
                .Where(c => c.IsVisible && c.StartTime <= DateTime.Now &&
                    (c.EndTime >= DateTime.Now ||
                        c.Type == ContestType.OnlinePractialExam &&
                        c.Participants.Any(p => p.IsOfficial && p.ContestEndTime >= DateTime.Now)));

        public IQueryable<Contest> GetAllInactive() =>
            this.contests
            .All()
            .Where(c => c.StartTime > DateTime.Now ||
                (c.EndTime < DateTime.Now && c.Type != ContestType.OnlinePractialExam) ||
                    !c.Participants.Any(p => p.ContestEndTime < DateTime.Now));

        public IQueryable<Contest> GetAllUpcoming() =>
            this.contests.All().Where(c => c.StartTime > DateTime.Now && c.IsVisible);

        public IQueryable<Contest> GetAllPast() =>
            this.contests.All().Where(c => c.EndTime < DateTime.Now && c.IsVisible);

        public IQueryable<Contest> GetAllVisible() => this.contests.All().Where(c => c.IsVisible);

        public IQueryable<Contest> GetAllWithDeleted() => this.contests.AllWithDeleted();

        public void DeleteById(int contestId)
        {
            this.contests.Delete(contestId);
            this.contests.SaveChanges();
        }

        public bool IsActiveById(int contestId)
        {
            var contest = this.contests.GetById(contestId);
            return contest != null && contest.IsActive;
        }
    }
}