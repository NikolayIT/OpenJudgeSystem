namespace OJS.Web.Areas.Administration.ViewModels.SubmissionType
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    public class SubmissionTypeAdministrationViewModel : AdministrationViewModel<SubmissionType>
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
        [StringLength(100, MinimumLength = 1)]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Селектирано")]
        public bool IsSelectedByDefault { get; set; }

        [DatabaseProperty]
        [Display(Name = "Execution стратегия")]
        public ExecutionStrategyType ExecutionStrategyType { get; set; }

        [DatabaseProperty]
        [Display(Name = "Компилатор")]
        public CompilerType CompilerType { get; set; }

        [DatabaseProperty]
        [Display(Name = "Допълнителни аргументи")]
        public string AdditionalCompilerArguments { get; set; }

        [DatabaseProperty]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [DatabaseProperty]
        [Display(Name = "Позволи качване на файлове")]
        public bool AllowBinaryFilesUpload { get; set; }

        [DatabaseProperty]
        [Display(Name = "Позволени файлове")]
        public string AllowedFileExtensions { get; set; }
    }
}