namespace OJS.Workers.Jobs.Models
{
    using System.Collections.Generic;

    using OJS.Common.Models;
    using OJS.Workers.ExecutionStrategies;

    public class SubmissionDto
    {
        public int Id { get; set; }

        public string AdditionalCompilerArguments { get; set; }

        public string CheckerAssemblyName { get; set; }

        public string CheckerParameter { get; set; }

        public string AllowedFileExtensions { get; set; }

        public string CheckerTypeName { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public string CompilerComment { get; set; }

        public string ProcessingComment { get; set; }

        public int MemoryLimit { get; set; }

        public int TimeLimit { get; set; }

        public byte[] FileContent { get; set; }

        public byte[] TaskSkeleton { get; set; }

        public CompilerType CompilerType { get; set; }

        public ExecutionStrategyType ExecutionStrategyType { get; set; }

        public IEnumerable<TestContext> Tests { get; set; }
    }
}