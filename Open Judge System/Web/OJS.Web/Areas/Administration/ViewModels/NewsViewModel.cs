namespace OJS.Web.Areas.Administration.ViewModels
{
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    public class NewsViewModel : AuditInfoViewModel
    {
        public static Expression<Func<News, NewsViewModel>> ViewModel
        {
            get
            {
                return news => new NewsViewModel
                {
                    Id = news.Id,
                    Title = news.Title,
                    Author = news.Author,
                    Source = news.Source,
                    Content = news.Content,
                    IsVisible = news.IsVisible,
                    CreatedOn = news.CreatedOn,
                    ModifiedOn = news.ModifiedOn
                };
            }
        }

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
    }
}