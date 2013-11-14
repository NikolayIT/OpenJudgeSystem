namespace OJS.Web.Areas.Administration.ViewModels.News
{
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    public class NewsAdministrationViewModel : AdministrationViewModel
    {
        public static Expression<Func<News, NewsAdministrationViewModel>> ViewModel
        {
            get
            {
                return news => new NewsAdministrationViewModel
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

        public News ToEntity
        {
            get
            {
                return new News
                {
                    Id = this.Id ?? default(int),
                    Title = this.Title,
                    Author = this.Author,
                    Source = this.Source,
                    Content = this.Content,
                    IsVisible = this.IsVisible,
                    CreatedOn = this.CreatedOn,
                    ModifiedOn = this.ModifiedOn
                };
            }
        }

        [Display(Name = "№")]
        [DefaultValue(null)]
        [UIHint("_NonEditable")]
        public int? Id { get; set; }

        [Display(Name = "Заглавие")]
        [Required(ErrorMessage = "Заглавието е задължително!")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Позволената дължина е между 6 и 100 символа")]
        [UIHint("_SingleLineText")]
        public string Title { get; set; }

        [Display(Name = "Автор")]
        [Required(ErrorMessage = "Автора е задължителен!")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Позволената дължина е между 2 и 25 символа")]
        [UIHint("_SingleLineText")]
        public string Author { get; set; }

        [Display(Name = "Източник")]
        [Required(ErrorMessage = "Съдържанието е задължително!")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Позволената дължина е между 6 и 100 символа")]
        [UIHint("_SingleLineText")]
        public string Source { get; set; }

        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Съдържанието е задължително!")]
        [StringLength(int.MaxValue, MinimumLength = 100, ErrorMessage = "Съдържанието трябва да бъде поне 100 символа")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [UIHint("_MultiLineText")]
        public string Content { get; set; }

        [Display(Name = "Видимост")]
        public bool IsVisible { get; set; }
    }
}