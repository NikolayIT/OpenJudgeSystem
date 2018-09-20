namespace OJS.Data
{
    using System;
    using System.Collections.Generic;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Data.Repositories;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class OjsData : IOjsData
    {
        private readonly IOjsDbContext context;

        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        public OjsData()
            : this(new OjsDbContext())
        {
        }

        protected OjsData(IOjsDbContext context) => this.context = context;

        public IDeletableEntityRepository<Contest> Contests => this.GetDeletableEntityRepository<Contest>();

        public IDeletableEntityRepository<Problem> Problems => this.GetDeletableEntityRepository<Problem>();

        public IDeletableEntityRepository<Event> Events => this.GetDeletableEntityRepository<Event>();

        public IDeletableEntityRepository<ContestCategory> ContestCategories =>
            this.GetDeletableEntityRepository<ContestCategory>();

        public IDeletableEntityRepository<ContestQuestion> ContestQuestions =>
            this.GetDeletableEntityRepository<ContestQuestion>();

        public IDeletableEntityRepository<ContestQuestionAnswer> ContestQuestionAnswers =>
            this.GetDeletableEntityRepository<ContestQuestionAnswer>();

        public ISubmissionsRepository Submissions => (SubmissionsRepository)this.GetRepository<Submission>();

        public IRepository<SubmissionType> SubmissionTypes => this.GetRepository<SubmissionType>();

        public IDeletableEntityRepository<SourceCode> SourceCodes => this.GetDeletableEntityRepository<SourceCode>();

        public IRepository<LecturerInContest> LecturersInContests => this.GetRepository<LecturerInContest>();

        public IRepository<LecturerInContestCategory> LecturersInContestCategories => this.GetRepository<LecturerInContestCategory>();

        public IRepository<Ip> Ips => this.GetRepository<Ip>();

        public IRepository<AccessLog> AccessLogs => this.GetRepository<AccessLog>();

        public IDeletableEntityRepository<FeedbackReport> FeedbackReports =>
            this.GetDeletableEntityRepository<FeedbackReport>();

        public IDeletableEntityRepository<Checker> Checkers => this.GetDeletableEntityRepository<Checker>();

        public IDeletableEntityRepository<ProblemResource> Resources =>
            this.GetDeletableEntityRepository<ProblemResource>();

        public IRepository<Setting> Settings => this.GetRepository<Setting>();

        public IUsersRepository Users => (UsersRepository)this.GetRepository<UserProfile>();

        public IRepository<IdentityRole> Roles => this.GetRepository<IdentityRole>();

        public IOjsDbContext Context => this.context;

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if the context has been disposed.</exception>
        public int SaveChanges() => this.context.SaveChanges();

        public void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.context?.Dispose();
            }
        }

        private IRepository<T> GetRepository<T>()
            where T : class
        {
            if (!this.repositories.ContainsKey(typeof(T)))
            {
                var type = typeof(EfGenericRepository<T>);

                if (typeof(T).IsAssignableFrom(typeof(Submission)))
                {
                    type = typeof(SubmissionsRepository);
                }

                if (typeof(T).IsAssignableFrom(typeof(UserProfile)))
                {
                    type = typeof(UsersRepository);
                }

                this.repositories.Add(typeof(T), Activator.CreateInstance(type, this.context));
            }

            return (IRepository<T>)this.repositories[typeof(T)];
        }

        private IDeletableEntityRepository<T> GetDeletableEntityRepository<T>()
            where T : class, IDeletableEntity, new()
        {
            if (!this.repositories.ContainsKey(typeof(T)))
            {
                var type = typeof(EfDeletableEntityRepository<T>);
                this.repositories.Add(typeof(T), Activator.CreateInstance(type, this.context));
            }

            return (IDeletableEntityRepository<T>)this.repositories[typeof(T)];
        }
    }
}