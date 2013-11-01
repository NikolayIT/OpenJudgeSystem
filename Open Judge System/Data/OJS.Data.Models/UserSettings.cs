namespace OJS.Data.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [ComplexType]
    public class UserSettings
    {
        public UserSettings()
        {
            this.DateOfBirth = null;
        }

        [Column("FirstName")]
        [MaxLength(30, ErrorMessage = "Въведеното име е твърде дълго")]
        [Display(Name = "Име")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string FirstName { get; set; }

        [Column("LastName")]
        [MaxLength(30, ErrorMessage = "Въведената фамилия е твърде дълга")]
        [Display(Name = "Фамилия")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string LastName { get; set; }

        [Column("City")]
        [MaxLength(30, ErrorMessage = "Въведеният град е твърде дълъг")]
        [Display(Name = "Град")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string City { get; set; }

        [Column("EducationalInstitution")]
        [MaxLength(50, ErrorMessage = "Въведеното образование е твърде дълго")]
        [Display(Name = "Образование")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string EducationalInstitution { get; set; }

        [Column("FacultyNumber")]
        [MaxLength(30, ErrorMessage = "Въведеният факултетен номер е твърде дълъг")]
        [Display(Name = "Факултетен номер")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string FacultyNumber { get; set; }

        [Column("DateOfBirth")]
        [Display(Name = "Дата на раждане")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [DataType(DataType.Date)]
        //// TODO: [Column(TypeName = "Date")] temporally disabled because of SQL Compact database not having "date" type
        public DateTime? DateOfBirth { get; set; }

        [Column("Company")]
        [MaxLength(30, ErrorMessage = "Въведената месторабота е твърде дълга")]
        [Display(Name = "Месторабота")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string Company { get; set; }

        [Column("JobTitle")]
        [MaxLength(30, ErrorMessage = "Въведената позиция е твърде дълга")]
        [Display(Name = "Позиция")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string JobTitle { get; set; }
        
        [NotMapped]
        [Display(Name = "Възраст")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public byte? Age
        {
            get
            {
                if (!this.DateOfBirth.HasValue)
                {
                    return null;
                }

                var birthDate = this.DateOfBirth.Value;
                var now = DateTime.Now;

                var age = now.Year - birthDate.Year;
                if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
                {
                    age--;
                }

                return (byte)age;
            }
        }
    }
}
