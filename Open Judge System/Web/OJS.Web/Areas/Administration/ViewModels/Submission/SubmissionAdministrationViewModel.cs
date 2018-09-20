namespace OJS.Web.Areas.Administration.ViewModels.Submission
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.Mvc;

    using MissingFeatures;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using OJS.Workers.Common.Extensions;

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.Submissions.ViewModels.SubmissionAdministration;

    public class SubmissionAdministrationViewModel : AdministrationViewModel<Submission>
    {
        private HttpPostedFileBase fileSubmission;

        [ExcludeFromExcel]
        public static Expression<Func<Submission, SubmissionAdministrationViewModel>> ViewModel
        {
            get
            {
                return sub => new SubmissionAdministrationViewModel
                {
                    Id = sub.Id,
                    ProblemId = sub.ProblemId,
                    ParticipantId = sub.ParticipantId,
                    SubmissionTypeId = sub.SubmissionTypeId,
                    AllowBinaryFilesUpload = sub.SubmissionType.AllowBinaryFilesUpload,
                    Content = sub.Content,
                    FileExtension = sub.FileExtension,
                    CreatedOn = sub.CreatedOn,
                    ModifiedOn = sub.ModifiedOn,
                };
            }
        }

        [DatabaseProperty]
        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Problem), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Problem_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(ProblemsComboBox)]
        public int? ProblemId { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Participant), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Participant_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(ParticipantDropDownList)]
        public int? ParticipantId { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Type), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Type_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SubmissionTypesDropDownList)]
        public int? SubmissionTypeId { get; set; }

        public bool? AllowBinaryFilesUpload { get; set; }

        [DatabaseProperty]
        [ScaffoldColumn(false)]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Content_required),
            ErrorMessageResourceType = typeof(Resource))]
        public byte[] Content { get; set; }

        [AllowHtml]
        [Display(Name = nameof(Resource.Content_as_string), ResourceType = typeof(Resource))]
        [UIHint(MultiLineText)]
        public string ContentAsString
        {
            get
            {
                if (this.AllowBinaryFilesUpload.HasValue && !this.AllowBinaryFilesUpload.Value)
                {
                    return this.Content.Decompress();
                }

                return null;
            }

            set
            {
                this.Content = value.Compress();
            }
        }

        [Display(Name = nameof(Resource.File_submission), ResourceType = typeof(Resource))]
        [ScaffoldColumn(false)]
        public HttpPostedFileBase FileSubmission
        {
            get
            {
                return this.fileSubmission;
            }

            set
            {
                this.fileSubmission = value;
                this.Content = value.InputStream.ToByteArray();
                this.FileExtension = value.FileName.GetFileExtension();
            }
        }

        [DatabaseProperty]
        public string FileExtension { get; set; }
    }
}