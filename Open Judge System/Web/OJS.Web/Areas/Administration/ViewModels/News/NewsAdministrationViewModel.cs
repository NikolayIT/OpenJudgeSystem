namespace OJS.Web.Areas.Administration.ViewModels.News
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    public class NewsAdministrationViewModel : AdministrationViewModel<News>
    {
        [ExcludeFromExcelAttribute]
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

        [DatabaseProperty]
        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [DatabaseProperty]
        [Display(Name = "Заглавие")]
        [Required(ErrorMessage = "Заглавието е задължително!")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Позволената дължина е между 6 и 100 символа")]
        [UIHint("SingleLineText")]
        public string Title { get; set; }

        [DatabaseProperty]
        [Display(Name = "Автор")]
        [Required(ErrorMessage = "Авторът е задължителен!")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Позволената дължина е между 2 и 25 символа")]
        [UIHint("SingleLineText")]
        public string Author { get; set; }

        [DatabaseProperty]
        [Display(Name = "Източник")]
        [Required(ErrorMessage = "Източникът е задължителен!")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Позволената дължина е между {2} и {1} символа")]
        [UIHint("SingleLineText")]
        public string Source { get; set; }

        [AllowHtml]
        [DatabaseProperty]
        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Съдържанието е задължително!")]
        [StringLength(int.MaxValue, MinimumLength = 100, ErrorMessage = "Съдържанието трябва да бъде поне 100 символа")]
        [DataType(DataType.MultilineText)]
        [UIHint("HtmlContent")]
        public string Content { get; set; }

        [Display(Name = "Видимост")]
        [DatabaseProperty]
        public bool IsVisible { get; set; }
    }
}