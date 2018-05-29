namespace OJS.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Common;

    [ComplexType]
    public class UserSettings
    {
        public UserSettings()
        {
            this.DateOfBirth = null;
        }

        [Column("FirstName")]
        [MaxLength(GlobalConstants.NameMaxLength)]
        public string FirstName { get; set; }

        [Column("LastName")]
        [MaxLength(GlobalConstants.NameMaxLength)]
        public string LastName { get; set; }

        [Column("City")]
        [MinLength(GlobalConstants.CityMinLength)]
        [MaxLength(GlobalConstants.CityMaxLength)]
        public string City { get; set; }

        [Column("EducationalInstitution")]
        public string EducationalInstitution { get; set; }

        [Column("FacultyNumber")]
        [MaxLength(GlobalConstants.FacultyNumberMaxLength)]
        public string FacultyNumber { get; set; }

        [Column("DateOfBirth")]
        [DataType(DataType.Date)]
        //// TODO: [Column(TypeName = "Date")] temporally disabled because of SQL Compact database not having "date" type
        public DateTime? DateOfBirth { get; set; }

        [Column("Company")]
        [MaxLength(GlobalConstants.CompanyMaxLength)]
        [MinLength(GlobalConstants.CompanyMinLength)]
        public string Company { get; set; }

        [Column("JobTitle")]
        [MaxLength(GlobalConstants.JobTitleMaxLength)]
        [MinLength(GlobalConstants.JobTitleMinLength)]
        public string JobTitle { get; set; }

        [NotMapped]
        public byte? Age => Calculator.Age(this.DateOfBirth);
    }
}
