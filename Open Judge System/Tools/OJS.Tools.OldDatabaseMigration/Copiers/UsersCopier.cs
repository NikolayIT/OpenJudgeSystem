namespace OJS.Tools.OldDatabaseMigration.Copiers
{
    using System;
    using System.Linq;

    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Data.Models;

    internal sealed class UsersCopier : ICopier
    {
        public void Copy(OjsDbContext context, TelerikContestSystemEntities oldDb)
        {
            context.Configuration.AutoDetectChangesEnabled = false;
            var users =
                oldDb.Users.OrderBy(x => x.Id).Select(
                    x => new
                        {
                            x.aspnet_Users.UserName,
                            x.aspnet_Users.aspnet_Membership.Email,
                            Profile = x,
                            HasAnyParticipations = x.Participants.Any(),
                            CreatedOn = x.aspnet_Users.aspnet_Membership.CreateDate,
                            x.Id,
                        })
                    .Where(x => x.HasAnyParticipations);
                    //// .Where(x => !x.UserName.StartsWith("sample@email.tst"))
                    //// .Where(x => !x.Email.StartsWith("sample@email.tst"))
                    //// .Where(x => x.Profile.LastName != "111-222-1933email@address.com")
                    //// .Where(x => !x.UserName.Contains("\'") && !x.UserName.Contains("\\") && !x.UserName.Contains("/") && !x.UserName.Contains("%") && !x.UserName.Contains(")"));

            foreach (var oldUser in users)
            {
                var dateOfBirth = oldUser.Profile.DateOfBirth;
                if (dateOfBirth.HasValue && dateOfBirth.Value.Year < 1900)
                {
                    dateOfBirth = null;
                }

                if (dateOfBirth.HasValue && dateOfBirth.Value.Year > 2006)
                {
                    dateOfBirth = null;
                }

                var user = new UserProfile(oldUser.UserName.Trim(), oldUser.Email.Trim())
                {
                    UserSettings = new UserSettings
                    {
                        FirstName = oldUser.Profile.FirstName.Trim().MaxLength(30),
                        LastName = oldUser.Profile.LastName.Trim().MaxLength(30),
                        City = oldUser.Profile.City.MaxLength(30),
                        DateOfBirth = dateOfBirth,
                        EducationalInstitution = oldUser.Profile.EducationalInstitution.MaxLength(50),
                        FacultyNumber = oldUser.Profile.FacultyNumber.MaxLength(30),
                        Company = oldUser.Profile.Company.MaxLength(30),
                        JobTitle = oldUser.Profile.JobTitle.MaxLength(30),
                    },
                    IsGhostUser = true,
                    OldId = oldUser.Id,
                    PreserveCreatedOn = true,
                    CreatedOn = oldUser.CreatedOn,
                };

                context.Users.Add(user);
            }

            try
            {
                context.SaveChanges();
            }
            catch
            {
                // ((System.Data.Entity.Validation.DbEntityValidationException)$exception).EntityValidationErrors.First().ValidationErrors.First()
                throw;
            }

            context.Configuration.AutoDetectChangesEnabled = true;
        }
    }
}
