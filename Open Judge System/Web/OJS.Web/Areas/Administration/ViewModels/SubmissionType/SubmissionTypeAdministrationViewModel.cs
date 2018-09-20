namespace OJS.Web.Areas.Administration.ViewModels.SubmissionType
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Workers.Common.Models;

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.SubmissionTypes.ViewModels.SubmissionTypeAdministration;

    public class SubmissionTypeAdministrationViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<SubmissionType, SubmissionTypeAdministrationViewModel>> ViewModel
        {
            get
            {
                return st => new SubmissionTypeAdministrationViewModel
                {
                    Id = st.Id,
                    Name = st.Name,
                    IsSelectedByDefault = st.IsSelectedByDefault,
                    ExecutionStrategyType = st.ExecutionStrategyType,
                    CompilerType = st.CompilerType,
                    AdditionalCompilerArguments = st.AdditionalCompilerArguments,
                    Description = st.Description,
                    AllowBinaryFilesUpload = st.AllowBinaryFilesUpload,
                    AllowedFileExtensions = st.AllowedFileExtensions
                };
            }
        }

        [DatabaseProperty]
        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceName = nameof(Resource.Name_required),
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.SubmissionTypeNameMaxLength,
            MinimumLength = GlobalConstants.SubmissionTypeNameMinLength,
            ErrorMessageResourceName = nameof(Resource.Name_length),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Is_selected), ResourceType = typeof(Resource))]
        public bool IsSelectedByDefault { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Execution_strategy_type), ResourceType = typeof(Resource))]
        [UIHint(EnumDropDown)]
        public ExecutionStrategyType ExecutionStrategyType { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Compiler_type), ResourceType = typeof(Resource))]
        [UIHint(EnumDropDown)]
        public CompilerType CompilerType { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Additional_compiler_arguments), ResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string AdditionalCompilerArguments { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Description), ResourceType = typeof(Resource))]
        [UIHint(MultiLineText)]
        public string Description { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Allow_binary_files_upload), ResourceType = typeof(Resource))]
        public bool AllowBinaryFilesUpload { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Allowed_file_extensions), ResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string AllowedFileExtensions { get; set; }

        public SubmissionType GetEntityModel(SubmissionType model = null)
        {
            model = model ?? new SubmissionType();
            model.Id = this.Id;
            model.Name = this.Name;
            model.IsSelectedByDefault = this.IsSelectedByDefault;
            model.ExecutionStrategyType = this.ExecutionStrategyType;
            model.CompilerType = this.CompilerType;
            model.AdditionalCompilerArguments = this.AdditionalCompilerArguments;
            model.Description = this.Description;
            model.AllowBinaryFilesUpload = this.AllowBinaryFilesUpload;
            model.AllowedFileExtensions = this.AllowedFileExtensions;
            return model;
        }
    }
}