namespace OJS.Workers.Agent.ExecutionStrategies
{
    using OJS.Common.Models;

    public class CSharpHighQualityCodeStrategy : ExecutionStrategyBase
    {
        public override void DoWork(CompilerType compilerType, string compilerAdditionalArguments, byte[] compilerContent, string checkerAssemblyName, string checkerTypeName, string checkerParameter, int timeLimit, int memoryLimit)
        {
            // No compilation
            // No execution
            // Just run style cop and evaluate result
        }
    }
}
