namespace OJS.Services.Common.HttpRequester.Models.Users
{
    using System;

    using OJS.Data.Models;

    public class ExternalUserInfoModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string City { get; set; }

        public string EducationalInstitution { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Company { get; set; }

        public string JobTitle { get; set; }

        public Guid? ForgottenPasswordToken { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        public UserProfile Entity =>
            new UserProfile
            {
                Id = this.Id,
                UserName = this.UserName,
                PasswordHash = this.PasswordHash,
                SecurityStamp = this.SecurityStamp,
                Email = this.Email,
                ForgottenPasswordToken = this.ForgottenPasswordToken,
                IsDeleted = this.IsDeleted,
                DeletedOn = this.DeletedOn,
                CreatedOn = this.CreatedOn,
                ModifiedOn = this.ModifiedOn,
                UserSettings = new UserSettings
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    DateOfBirth = this.DateOfBirth,
                    City = this.City,
                    EducationalInstitution = this.EducationalInstitution,
                    Company = this.Company,
                    JobTitle = this.JobTitle
                }
            };
    }
}
