namespace OJS.Web.Areas.Administration.ViewModels.Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Common;
    using OJS.Workers.Common.Extensions;

    using Resource = Resources.Areas.Administration.Tests.ViewModels.TestAdministration;

    public class TestViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<Test, TestViewModel>> FromTest =>
            test => new TestViewModel
            {
                Id = test.Id,
                InputData = test.InputData,
                OutputData = test.OutputData,
                OrderBy = test.OrderBy,
                ProblemId = test.Problem.Id,
                HideInput = test.HideInput,
                ProblemName = test.Problem.Name,
                TestRunsCount = test.TestRuns.Count,
                Type = test.IsTrialTest
                    ? TestType.Trial
                    : test.IsOpenTest
                        ? TestType.Open
                        : TestType.Standard
            };

        public int Id { get; set; }

        [Display(Name = nameof(Resource.Problem_name), ResourceType = typeof(Resource))]
        public string ProblemName { get; set; }

        [Display(Name = nameof(Resource.Input), ResourceType = typeof(Resource))]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceName = nameof(Resource.Input_required),
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            int.MaxValue,
            MinimumLength = GlobalConstants.TestInputMinLength)]
        public string Input
        {
            get
            {
                if (this.InputData == null)
                {
                    return string.Empty;
                }

                var result = this.InputData.Decompress();
                return result.Length > 20 ? result.Substring(0, 20) : result;
            }

            set => this.InputData = value.Compress();
        }

        [Display(Name = nameof(Resource.Input), ResourceType = typeof(Resource))]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceName = nameof(Resource.Input_required),
            ErrorMessageResourceType = typeof(Resource))]
        [ScriptIgnore]
        [StringLength(
            int.MaxValue,
            MinimumLength = GlobalConstants.TestInputMinLength)]
        public string InputFull
        {
            get
            {
                if (this.InputData == null)
                {
                    return string.Empty;
                }

                var result = this.InputData.Decompress();
                return result;
            }

            set => this.InputData = value.Compress();
        }

        [Display(Name = nameof(Resource.Output), ResourceType = typeof(Resource))]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceName = nameof(Resource.Output_required),
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            int.MaxValue,
            MinimumLength = GlobalConstants.TestOutputMinLength)]
        public string Output
        {
            get
            {
                if (this.OutputData == null)
                {
                    return string.Empty;
                }

                var result = this.OutputData.Decompress();
                return result.Length > 20 ? result.Substring(0, 20) : result;
            }

            set => this.OutputData = value.Compress();
        }

        [Display(Name = nameof(Resource.Output), ResourceType = typeof(Resource))]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceName = nameof(Resource.Output_required),
            ErrorMessageResourceType = typeof(Resource))]
        [ScriptIgnore]
        [StringLength(
            int.MaxValue,
            MinimumLength = GlobalConstants.TestOutputMinLength)]
        public string OutputFull
        {
            get
            {
                if (this.OutputData == null)
                {
                    return string.Empty;
                }

                var result = this.OutputData.Decompress();
                return result;
            }

            set => this.OutputData = value.Compress();
        }

        [Display(Name = nameof(Resource.Retest_problem), ResourceType = typeof(Resource))]
        public bool RetestProblem { get; set; }

        [Display(Name = nameof(Resource.Hide_input), ResourceType = typeof(Resource))]
        public bool HideInput { get; set; }

        [Display(Name = nameof(Resource.Trial_test_name), ResourceType = typeof(Resource))]
        public string TrialTestName =>
            this.Type == TestType.Trial
                ? Resource.Practice
                : Resource.Contest;

        [Display(Name = nameof(Resource.Type_test_name), ResourceType = typeof(Resource))]
        public TestType Type { get; set; }

        [Display(Name = nameof(Resource.Open_test_name), ResourceType = typeof(Resource))]
        public string TypeName { get; set; }

        public IEnumerable<SelectListItem> AllTypes { get; set; }

        [Display(Name = nameof(Resource.Order), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Order_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("NumericTextBox")]
        public int OrderBy { get; set; }

        [Display(Name = nameof(Resource.Problem_id), ResourceType = typeof(Resource))]
        public int ProblemId { get; set; }

        [Display(Name = nameof(Resource.Test_runs_count), ResourceType = typeof(Resource))]
        public int TestRunsCount { get; set; }

        internal byte[] InputData { get; set; }

        internal byte[] OutputData { get; set; }
    }
}