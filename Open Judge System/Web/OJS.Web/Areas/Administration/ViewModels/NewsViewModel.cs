namespace OJS.Web.Areas.Administration.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class NewsViewModel
    {
        [Display(Name = "№")]
        public int Id { get; set; }

        [Display(Name = "Заглавие")]
        [Required(ErrorMessage = "Заглавието е задължително!")]
        public string Title { get; set; }

        [Display(Name = "Автор")]
        [Required(ErrorMessage = "Автора е задължителен!")]
        public string Author { get; set; }

        [Display(Name = "Източник")]
        public string Source { get; set; }

        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Съдържанието е задължително!")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [Display(Name = "Видимост")]
        public bool IsVisible { get; set; }

        [Display(Name = "Изтрит?")]
        [Editable(false)]
        public bool IsDeleted { get; set; }

        [Display(Name = "Дата на изтриване")]
        [Editable(false)]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedOn { get; set; }
    }
}