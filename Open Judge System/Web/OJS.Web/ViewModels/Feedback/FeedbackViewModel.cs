namespace OJS.Web.ViewModels.Feedback
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Feedback.ViewModels.FeedbackViewModels;

    public class FeedbackViewModel
    {
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(
            Name = nameof(Resource.Email),
            ResourceType = typeof(Resource))]
        [EmailAddress(
            ErrorMessage = null,
            ErrorMessageResourceName = nameof(Resource.Invalid_email),
            ErrorMessageResourceType = typeof(Resource))]
        public string Email { get; set; }

        [UIHint(MultiLineText)]
        [Display(
            Name = nameof(Resource.Content),
            ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Content_required),
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            int.MaxValue,
            MinimumLength = GlobalConstants.FeedbackContentMinLength,
            ErrorMessageResourceName = nameof(Resource.Content_too_short),
            ErrorMessageResourceType = typeof(Resource))]
        public string Content { get; set; }
    }
}