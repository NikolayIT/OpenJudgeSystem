namespace OJS.Web.Areas.Administration.ViewModels.Test
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Script.Serialization;

    using OJS.Common.DataAnnotations;
    using OJS.Common.Extensions;
    using OJS.Data.Models;

    public class TestViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<Test, TestViewModel>> FromTest
        {
            get
            {
                return test => new TestViewModel
                {
                    Id = test.Id,
                    InputData = test.InputData,
                    OutputData = test.OutputData,
                    IsTrialTest = test.IsTrialTest,
                    OrderBy = test.OrderBy,
                    ProblemId = test.Problem.Id,
                    ProblemName = test.Problem.Name,
                    TestRunsCount = test.TestRuns.Count
                };
            }
        }

        public int Id { get; set; }

        [Display(Name = "Задача")]
        public string ProblemName { get; set; }

        [Display(Name = "Вход")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Входа е задължителен!", AllowEmptyStrings = false)]
        [StringLength(int.MaxValue, MinimumLength = 1)]
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

            set
            {
                this.InputData = value.Compress();
            }
        }

        [Display(Name = "Вход")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Входа е задължителен!", AllowEmptyStrings = false)]
        [ScriptIgnore]
        [StringLength(int.MaxValue, MinimumLength = 1)]
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

            set
            {
                this.InputData = value.Compress();
            }
        }

        [Display(Name = "Изход")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Изхода е задължителен!", AllowEmptyStrings = false)]
        [StringLength(int.MaxValue, MinimumLength = 1)]
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

            set
            {
                this.OutputData = value.Compress();
            }
        }

        [Display(Name = "Изход")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Изхода е задължителен!", AllowEmptyStrings = false)]
        [ScriptIgnore]
        [StringLength(int.MaxValue, MinimumLength = 1)]
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

            set
            {
                this.OutputData = value.Compress();
            }
        }

        [Display(Name = "Пробен тест")]
        public string TrialTestName
        {
            get
            {
                return this.IsTrialTest ? "Пробен" : "Състезателен";
            }
        }

        [Display(Name = "Пробен тест")]
        public bool IsTrialTest { get; set; }

        [Display(Name = "Подредба")]
        [Required(ErrorMessage = "Подредбата е задължителна!")]
        [RegularExpression(@"(0|[1-9]{1}[0-9]{0,8}|[1]{1}[0-9]{1,9}|[-]{1}[2]{1}([0]{1}[0-9]{8}|[1]{1}([0-3]{1}[0-9]{7}|[4]{1}([0-6]{1}[0-9]{6}|[7]{1}([0-3]{1}[0-9]{5}|[4]{1}([0-7]{1}[0-9]{4}|[8]{1}([0-2]{1}[0-9]{3}|[3]{1}([0-5]{1}[0-9]{2}|[6]{1}([0-3]{1}[0-9]{1}|[4]{1}[0-8]{1}))))))))|(\+)?[2]{1}([0]{1}[0-9]{8}|[1]{1}([0-3]{1}[0-9]{7}|[4]{1}([0-6]{1}[0-9]{6}|[7]{1}([0-3]{1}[0-9]{5}|[4]{1}([0-7]{1}[0-9]{4}|[8]{1}([0-2]{1}[0-9]{3}|[3]{1}([0-5]{1}[0-9]{2}|[6]{1}([0-3]{1}[0-9]{1}|[4]{1}[0-7]{1})))))))))")]
        public int OrderBy { get; set; }

        [Display(Name = "ID на задача")]
        public int ProblemId { get; set; }

        [Display(Name = "Брой изпълнения")]
        public int TestRunsCount { get; set; }

        internal byte[] InputData { get; set; }

        internal byte[] OutputData { get; set; }
    }
}