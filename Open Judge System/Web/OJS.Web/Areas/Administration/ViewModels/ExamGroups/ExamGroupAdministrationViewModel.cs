namespace OJS.Web.Areas.Administration.ViewModels.ExamGroups
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

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.ExamGroups.ViewModels.ExamGroupAdministration;
    using SharedResource = Resources.Areas.Administration.Shared.EditorTemplatesGeneral;

    public class ExamGroupAdministrationViewModel : AdministrationViewModel<ExamGroup>
    {
        public static Expression<Func<ExamGroup, ExamGroupAdministrationViewModel>> FromExamGroup =>
            eg => new ExamGroupAdministrationViewModel
            {
                Id = eg.Id,
                Name = eg.Name,
                ExternalExamGroupId = eg.ExternalExamGroupId,
                ExternalAppId = eg.ExternalAppId,
                ContestId = eg.ContestId,
                ContestName = eg.Contest.Name
            };

        [DatabaseProperty]
        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(SharedResource.Name), ResourceType = typeof(SharedResource))]
        [Required(
            ErrorMessageResourceName = nameof(SharedResource.Name_required),
            ErrorMessageResourceType = typeof(SharedResource))]
        [StringLength(
            GlobalConstants.ExamGroupNameMaxLength,
            MinimumLength = GlobalConstants.ExamGroupNameMinLength,
            ErrorMessageResourceName = nameof(SharedResource.Name_length),
            ErrorMessageResourceType = typeof(SharedResource))]
        [UIHint(SingleLineText)]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.External_id), ResourceType = typeof(Resource))]
        [HiddenInput(DisplayValue = false)]
        public int? ExternalExamGroupId { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Extenral_app_id), ResourceType = typeof(Resource))]
        [HiddenInput(DisplayValue = false)]
        public string ExternalAppId { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Contest), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Contest_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(ContestsComboBox)]
        public int? ContestId { get; set; }

        [Display(Name = nameof(Resource.Contest), ResourceType = typeof(Resource))]
        [HiddenInput(DisplayValue = false)]
        public string ContestName { get; set; }
    }
}