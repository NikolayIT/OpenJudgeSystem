namespace OJS.Workers.Agent.ExecutionStrategies
{
    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class CompileExecuteAndCheckStrategy : ExecutionStrategyBase
    {
        public override void DoWork(
            CompilerType compilerType,
            string compilerAdditionalArguments,
            byte[] compilerContent,
            string checkerAssemblyName,
            string checkerTypeName,
            string checkerParameter,
            int timeLimit,
            int memoryLimit)
        {
            // Compile
            var compileResult = this.Compile(compilerType, compilerAdditionalArguments, compilerContent);
            if (!compileResult.IsCompiledSuccessfully)
            {
                // Unsuccessful compilation
            }

            // Create executor
            IExecutor executor = new RestrictedProcessExecutor();

            // Create checker
            var checker = this.CreateChecker(checkerAssemblyName, checkerTypeName, checkerParameter);

            foreach (var a in string.Empty)
            {
                var executionResult = executor.Execute(string.Empty, string.Empty, timeLimit, memoryLimit); // TODO: pass compiler output
                if (executionResult.Type != ProcessExecutionResultType.Success)
                {
                    // Unsuccessful execution
                }

                var checkerResult = checker.Check(string.Empty, executionResult.ReceivedOutput, string.Empty);
                if (!checkerResult.IsCorrect)
                {
                    // Wrong answer
                }
            }
        }
    }
}
