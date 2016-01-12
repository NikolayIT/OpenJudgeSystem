namespace OJS.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Data.Configurations;
    using OJS.Data.Contracts;
    using OJS.Data.Contracts.CodeFirstConventions;
    using OJS.Data.Models;

    public class OjsDbContext : IdentityDbContext<UserProfile>, IOjsDbContext
    {
        public OjsDbContext()
            : this("DefaultConnection")
        {
        }

        public OjsDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public virtual IDbSet<Setting> Settings { get; set; }

        public virtual IDbSet<Contest> Contests { get; set; }

        public virtual IDbSet<Problem> Problems { get; set; }

        public virtual IDbSet<News> News { get; set; }

        public virtual IDbSet<Event> Events { get; set; }

        public virtual IDbSet<Participant> Participants { get; set; }

        public virtual IDbSet<ContestCategory> ContestCategories { get; set; }

        public virtual IDbSet<ContestQuestion> ContestQuestions { get; set; }

        public virtual IDbSet<ContestQuestionAnswer> ContestQuestionAnswers { get; set; }

        public virtual IDbSet<Checker> Checkers { get; set; }

        public virtual IDbSet<Test> Tests { get; set; }

        public virtual IDbSet<Submission> Submissions { get; set; }

        public virtual IDbSet<SubmissionType> SubmissionTypes { get; set; }

        public virtual IDbSet<SourceCode> SourceCodes { get; set; }

        public virtual IDbSet<TestRun> TestRuns { get; set; }

        public virtual IDbSet<FeedbackReport> FeedbackReports { get; set; }

        public virtual IDbSet<ParticipantAnswer> ParticipantAnswers { get; set; }

        public virtual IDbSet<LecturerInContest> LecturersInContests { get; set; }

        public virtual IDbSet<Ip> Ips { get; set; }

        public virtual IDbSet<AccessLog> AccessLogs { get; set; }

        public DbContext DbContext => this;

        public override int SaveChanges()
        {
            this.ApplyAuditInfoRules();

            ////// Use this to see Database validation errors
            ////try
            ////{
            ////    return base.SaveChanges();
            ////}
            ////catch (DbEntityValidationException databeseException)
            ////{
            ////    foreach (var validationErrors in databeseException.EntityValidationErrors)
            ////    {
            ////        foreach (var validationError in validationErrors.ValidationErrors)
            ////        {
            ////            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
            ////        }
            ////    }
            ////}

            return base.SaveChanges();
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
                        "News",
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
                        "ContestCategories",
                    };

            foreach (var tableName in tableNames)
            {
                this.Database.ExecuteSqlCommand(string.Format("DELETE FROM {0}", tableName));
                try
                {
                    this.Database.ExecuteSqlCommand(string.Format("DBCC CHECKIDENT ('{0}', RESEED, 0)", tableName));
                }
                catch
                {
                    // The table does not contain an identity column
                }
            }

            this.SaveChanges();
        }

        public new IDbSet<T> Set<T>()
            where T : class
        {
            return base.Set<T>();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new IsUnicodeAttributeConvention());

            modelBuilder.Configurations.Add(new TestRunConfiguration());
            modelBuilder.Configurations.Add(new ParticipantAnswersConfiguration());
            modelBuilder.Configurations.Add(new UserProfileConfiguration());

            base.OnModelCreating(modelBuilder); // Without this call EntityFramework won't be able to configure the identity model
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
    }
}
