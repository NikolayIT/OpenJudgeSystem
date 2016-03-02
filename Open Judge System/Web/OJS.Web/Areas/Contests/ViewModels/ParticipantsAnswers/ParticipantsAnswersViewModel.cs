namespace OJS.Web.Areas.Contests.ViewModels.ParticipantsAnswers
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Common.DataAnnotations;

    public class ParticipantsAnswersViewModel
    {
        [ExcludeFromExcel]
        public int Id { get; set; }

        [Display(Name = "Потребител")]
        public string ParticipantUsername { get; set; }

        [Display(Name = "Пълно Име")]
        public string ParticipantFullName { get; set; }

        [Display(Name = "Преподавател")]
        public string Answer { get; set; }

        [Display(Name = "Точки")]
        public int Points { get; set; }
    }
}
