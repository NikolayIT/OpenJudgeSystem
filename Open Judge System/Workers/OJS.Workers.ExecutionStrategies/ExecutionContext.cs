namespace OJS.Workers.ExecutionStrategies
{
    using System.Collections.Generic;

    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class ExecutionContext
    {
        public int SubmissionId { get; set; }

        public CompilerType CompilerType { get; set; }

        public string AdditionalCompilerArguments { get; set; }

        public string Code
        {
            get
            {
                return this.FileContent.Decompress();
            }
        }

        public byte[] FileContent { get; set; }

        public string AllowedFileExtensions { get; set; }

        public IEnumerable<TestContext> Tests { get; set; }

        public int TimeLimit { get; set; }

        public int MemoryLimit { get; set; }

        public string CheckerAssemblyName { get; set; }

        public string CheckerTypeName { get; set; }

        public string CheckerParameter { get; set; }
    }
}