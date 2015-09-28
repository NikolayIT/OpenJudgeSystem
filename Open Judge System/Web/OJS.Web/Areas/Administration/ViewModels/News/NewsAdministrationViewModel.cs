namespace OJS.Web.Areas.Administration.ViewModels.News
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using Resource = Resources.Areas.Administration.News.ViewModels.NewsAdministration;

    public class NewsAdministrationViewModel : AdministrationViewModel<News>
    {
        [ExcludeFromExcel]
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
        [Display(Name = "Title", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Title_required",
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.NewsTitleMaxLength,
            MinimumLength = GlobalConstants.NewsTitleMinLength, 
            ErrorMessageResourceName = "Title_length",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("SingleLineText")]
        public string Title { get; set; }

        [DatabaseProperty]
        [Display(Name = "Author", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Author_required",
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.NewsAuthorNameMaxLength, 
            MinimumLength = GlobalConstants.NewsAuthorNameMinLength,
            ErrorMessageResourceName = "Author_length",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("SingleLineText")]
        public string Author { get; set; }

        [DatabaseProperty]
        [Display(Name = "Source", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Source_required",
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.NewsSourceMaxLength, 
            MinimumLength = GlobalConstants.NewsSourceMinLength,
            ErrorMessageResourceName = "Source_length",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("SingleLineText")]
        public string Source { get; set; }

        [AllowHtml]
        [DatabaseProperty]
        [Display(Name = "Content", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Content_required",
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            int.MaxValue, 
            MinimumLength = GlobalConstants.NewsContentMinLength,
            ErrorMessageResourceName = "Content_length",
            ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.MultilineText)]
        [UIHint("HtmlContent")]
        public string Content { get; set; }

        [Display(Name = "Visibility", ResourceType = typeof(Resource))]
        [DatabaseProperty]
        public bool IsVisible { get; set; }
    }
}