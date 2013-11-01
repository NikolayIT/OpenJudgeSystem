namespace OJS.Workers.Agent.ExecutionStrategies
{
    using System;
    using System.IO;

    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Compilers;

    // TODO: Implement template method pattern
    public abstract class ExecutionStrategyBase
    {
        public ICompiler CreateCompiler(CompilerType type)
        {
            switch (type)
            {
                case CompilerType.None:
                    return null;
                case CompilerType.CSharp:
                    return new CSharpCompiler();
                case CompilerType.MsBuild:
                    return null;
                case CompilerType.CPlusPlus:
                    return new CPlusPlusCompiler();
                case CompilerType.Java:
                    return null;
                default:
                    return null;
            }
        }

        public string GetCompilerPath(CompilerType type)
        {
            // TODO: Implement
            switch (type)
            {
                case CompilerType.None:
                    return string.Empty;
                case CompilerType.CSharp:
                    return string.Empty;
                case CompilerType.MsBuild:
                    return string.Empty;
                case CompilerType.CPlusPlus:
                    return string.Empty;
                case CompilerType.Java:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        public CompileResult Compile(CompilerType type, string additionalArguments, byte[] compilerContent)
        {
            try
            {
                var compiler = this.CreateCompiler(type);
                var compilerPath = this.GetCompilerPath(type);
                var inputFile = Path.GetTempFileName();
                File.WriteAllBytes(inputFile, compilerContent);

                var compilerResult = compiler.Compile(compilerPath, inputFile, additionalArguments);
                return compilerResult;
            }
            catch (Exception ex)
            {
                return new CompileResult(
                    false,
                    string.Format("Exception is thrown in compiler.Compile(): {0}", ex.Message));
            }
        }

        public IChecker CreateChecker(string assemblyName, string typeName, string parameter)
        {
            var checker = (IChecker)Activator.CreateInstance(assemblyName, typeName).Unwrap();
            if (checker == null)
            {
                return null;
            }

            if (parameter != null)
            {
                checker.SetParameter(parameter);
            }

            return checker;
        }

        public abstract void DoWork(
            CompilerType compilerType,
            string compilerAdditionalArguments,
            byte[] compilerContent,
            string checkerAssemblyName,
            string checkerTypeName,
            string checkerParameter,
            int timeLimit,
            int memoryLimit);
    }
}
