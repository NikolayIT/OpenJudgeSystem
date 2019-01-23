namespace OJS.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Validation;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Common.Constants;
    using OJS.Data.Configurations;
    using OJS.Data.Contracts;
    using OJS.Data.Contracts.CodeFirstConventions;
    using OJS.Data.Models;

    public class OjsDbContext : IdentityDbContext<UserProfile>, IOjsDbContext
    {
        public OjsDbContext()
            : this(AppSettingConstants.DefaultDbConnectionStringName)
        {
        }

        protected OjsDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString, throwIfV1Schema: false)
        {
        }

        public virtual IDbSet<Setting> Settings { get; set; }

        public virtual IDbSet<Contest> Contests { get; set; }

        public virtual IDbSet<ExamGroup> ExamGroups { get; set; }

        public virtual IDbSet<Problem> Problems { get; set; }

        public virtual IDbSet<ProblemGroup> ProblemGroups { get; set; }

        public virtual IDbSet<Event> Events { get; set; }

        public virtual IDbSet<Participant> Participants { get; set; }

        public virtual IDbSet<ParticipantScore> ParticipantScores { get; set; }

        public virtual IDbSet<ContestCategory> ContestCategories { get; set; }

        public virtual IDbSet<ContestQuestion> ContestQuestions { get; set; }

        public virtual IDbSet<ContestQuestionAnswer> ContestQuestionAnswers { get; set; }

        public virtual IDbSet<Checker> Checkers { get; set; }

        public virtual IDbSet<Test> Tests { get; set; }

        public virtual IDbSet<Submission> Submissions { get; set; }

        public virtual IDbSet<SubmissionForProcessing> SubmissionsForProcessing { get; set; }

        public virtual IDbSet<SubmissionType> SubmissionTypes { get; set; }

        public virtual IDbSet<SourceCode> SourceCodes { get; set; }

        public virtual IDbSet<TestRun> TestRuns { get; set; }

        public virtual IDbSet<FeedbackReport> FeedbackReports { get; set; }

        public virtual IDbSet<ParticipantAnswer> ParticipantAnswers { get; set; }

        public virtual IDbSet<LecturerInContest> LecturersInContests { get; set; }

        public virtual IDbSet<LecturerInContestCategory> LecturersInContestCategories { get; set; }

        public virtual IDbSet<Ip> Ips { get; set; }

        public virtual IDbSet<AccessLog> AccessLogs { get; set; }

        public DbContext DbContext => this;

        public override int SaveChanges()
        {
            this.ApplyAuditInfoRules();

#if DEBUG
            //// Use this to see DB validation errors
            return this.SaveChangesWithTracingDbExceptions();
#else
            return base.SaveChanges();
#endif
        }

        public void ClearDatabase()
        {
            /*
            // Possible solution to foreign key deletes: http://www.ridgway.co.za/articles/174.aspx
            // The above solution does not work with cyclic relations.
            */

            this.SaveChanges();
            var tableNames =
                /* this.Database.SqlQuery<string>(
                      "SELECT [TABLE_NAME] from information_schema.tables WHERE [TABLE_NAME] != '__MigrationHistory'")
                      .ToList();
                 */
                new List<string>
                    {
                        "ParticipantAnswers",
                        "FeedbackReports",
                        "AspNetUserRoles",
                        "AspNetRoles",
                        "AspNetUserLogins",
                        "AspNetUserClaims",
                        "Events",
                        "TestRuns",
                        "Submissions",
                        "Participants",
                        "AspNetUsers",
                        "Tests",
                        "Problems",
                        "Checkers",
                        "ContestQuestionAnswers",
                        "ContestQuestions",
                        "Contests",
                        "ContestCategories"
                    };

            foreach (var tableName in tableNames)
            {
                this.Database.ExecuteSqlCommand($"DELETE FROM {tableName}");
                try
                {
                    this.Database.ExecuteSqlCommand($"DBCC CHECKIDENT ('{tableName}', RESEED, 0)");
                }
                catch
                {
                    // The table does not contain an identity column
                }
            }

            this.SaveChanges();
        }

        public new IDbSet<T> Set<T>()
            where T : class => base.Set<T>();

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new IsUnicodeAttributeConvention());

            modelBuilder.Configurations.Add(new TestRunConfiguration());
            modelBuilder.Configurations.Add(new ParticipantAnswersConfiguration());
            modelBuilder.Configurations.Add(new ParticipantScoresConfiguration());
            modelBuilder.Configurations.Add(new UserProfileConfiguration());
            modelBuilder.Configurations.Add(new ProblemsConfiguration());

            ManyToManyTableNamesConfiguration.Configure(modelBuilder);

            // Without this call EntityFramework won't be able to configure the identity model
            base.OnModelCreating(modelBuilder);
        }

        private void ApplyAuditInfoRules()
        {
            // Approach via @julielerman: http://bit.ly/123661P
            foreach (var entry in
                this.ChangeTracker.Entries()
                    .Where(
                        e =>
                        e.Entity is IAuditInfo && ((e.State == EntityState.Added) || (e.State == EntityState.Modified))))
            {
                var entity = (IAuditInfo)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    if (!entity.PreserveCreatedOn)
                    {
                        entity.CreatedOn = DateTime.Now;
                        entity.ModifiedOn = null;
                    }
                }
                else
                {
                    entity.ModifiedOn = DateTime.Now;
                }
            }
        }

        private void ApplyDeletableEntityRules()
        {
            // Approach via @julielerman: http://bit.ly/123661P
            foreach (
                var entry in
                    this.ChangeTracker.Entries()
                        .Where(e => e.Entity is IDeletableEntity && (e.State == EntityState.Deleted)))
            {
                var entity = (IDeletableEntity)entry.Entity;

                entity.DeletedOn = DateTime.Now;
                entity.IsDeleted = true;
                entry.State = EntityState.Modified;
            }
        }

        private int SaveChangesWithTracingDbExceptions()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Exception currentException = ex;
                while (currentException != null)
                {
                    Trace.TraceError(currentException.Message);
                    currentException = currentException.InnerException;
                }

                throw;
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceError(
                            $"Property: {validationError.PropertyName}{Environment.NewLine} Error: {validationError.ErrorMessage}");
                    }
                }

                throw;
            }
        }
    }
}
