namespace OJS.Workers.ExecutionStrategies
{
    using System.Collections.Generic;

    using OJS.Common.Models;

    public class SubmissionsExecutorContext
    {
        public CompilerType CompilerType { get; set; }

        public string AdditionalCompilerArguments { get; set; }

        public string Code { get; set; }

        public List<TestContext> Tests { get; set; }

        public int TimeLimit { get; set; }

        public int MemoryLimit { get; set; }

        public string CheckerAssemblyName { get; set; }

        public string CheckerTypeName { get; set; }

        public string CheckerParameter { get; set; }
    }
}