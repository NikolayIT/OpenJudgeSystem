namespace OJS.Web.ViewModels.Feedback
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;

    using Resource = Resources.Feedback.ViewModels.FeedbackViewModels;

    public class FeedbackViewModel
    {
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(
            Name = "Email", 
            ResourceType = typeof(Resource))]
        [EmailAddress(
            ErrorMessage = null, 
            ErrorMessageResourceName = "Invalid_email", 
            ErrorMessageResourceType = typeof(Resource))]
        public string Email { get; set; }

        [UIHint("MultilineText")]
        [Display(
            Name = "Content", 
            ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Content_required", 
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            int.MaxValue, 
            MinimumLength = GlobalConstants.FeedbackContentMinLength, 
            ErrorMessageResourceName = "Content_too_short", 
            ErrorMessageResourceType = typeof(Resource))]
        public string Content { get; set; }
    }
}