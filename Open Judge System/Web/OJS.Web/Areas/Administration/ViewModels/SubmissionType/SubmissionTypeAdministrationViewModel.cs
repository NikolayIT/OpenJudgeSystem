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
        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително!", AllowEmptyStrings = false)]
        [StringLength(
            GlobalConstants.SubmissionTypeNameMaxLength, 
            MinimumLength = GlobalConstants.SubmissionTypeNameMinLength)]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Селектирано")]
        public bool IsSelectedByDefault { get; set; }

        [DatabaseProperty]
        [Display(Name = "Execution стратегия")]
        [UIHint("Enum")]
        public ExecutionStrategyType ExecutionStrategyType { get; set; }

        [DatabaseProperty]
        [Display(Name = "Компилатор")]
        [UIHint("Enum")]
        public CompilerType CompilerType { get; set; }

        [DatabaseProperty]
        [Display(Name = "Допълнителни аргументи")]
        [UIHint("SingleLineText")]
        public string AdditionalCompilerArguments { get; set; }

        [DatabaseProperty]
        [Display(Name = "Описание")]
        [UIHint("MultiLineText")]
        public string Description { get; set; }

        [DatabaseProperty]
        [Display(Name = "Позволи качване на файлове")]
        public bool AllowBinaryFilesUpload { get; set; }

        [DatabaseProperty]
        [Display(Name = "Позволени файлове")]
        [UIHint("SingleLineText")]
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