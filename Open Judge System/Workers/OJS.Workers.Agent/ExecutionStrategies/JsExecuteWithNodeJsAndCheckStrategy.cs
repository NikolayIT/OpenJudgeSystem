namespace OJS.Workers.Agent.ExecutionStrategies
{
    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class JsExecuteWithNodeJsAndCheckStrategy : ExecutionStrategyBase
    {
        public override void DoWork(CompilerType compilerType, string compilerAdditionalArguments, byte[] compilerContent, string checkerAssemblyName, string checkerTypeName, string checkerParameter, int timeLimit, int memoryLimit)
        {
            // Create executor
            IExecutor executor = new NodeJsExecutor();

            // Create checker
            var checker = this.CreateChecker(checkerAssemblyName, checkerTypeName, checkerParameter);

            // Foreach test check the result
            foreach (var a in string.Empty)
            {
                var executionResult = executor.Execute(string.Empty, string.Empty, timeLimit, memoryLimit); // TODO: pass compiler output
                checker.Check(string.Empty, string.Empty, string.Empty); // TODO: pass received output
            }
        }
    }
}
