namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.CSharp;
    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class CSharpTestRunnerExecutionStrategy : ExecutionStrategy
    {
        private const string TestRunnerTemplate = @"namespace LocalDefinedCSharpTestRunner
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class LocalCSharpTestRunner
    {
        private const string JsonTestsResult = @""{ """"stats"""" : { """"passes"""": #totalPasses# }, """"passing"""": [ #passingTests# ], """"failures"""" : [ { """"err"""": { """"message"""": """"#message#"""" } } ] }"";

        private class FuncTestResult
        {
            public int Index { get; set; }

            public bool Passed { get; set; }

            public string Error { get; set; }
        }

        private static readonly IReadOnlyCollection<Func<List<Type>, bool>> tests = new ReadOnlyCollection<Func<List<Type>, bool>>(new List<Func<List<Type>, bool>>
        { 
#allTests#
        });

        public static void Main()
        {
            var currentDirectory = Environment.CurrentDirectory;

            var allTypes = new List<Type>();

            foreach (var file in Directory.GetFiles(currentDirectory).Where(f => f.EndsWith("".dll"") || f.EndsWith("".exe"")))
            {
                var assembly = Assembly.LoadFrom(file);
                foreach (var type in assembly.GetTypes())
                {
                    allTypes.Add(type);
                }

                var referenced = assembly.GetReferencedAssemblies().Where(r => r.Name != ""mscorlib"" && r.Name != ""System.Core"");
                foreach (var reference in referenced)
                {
                    var refAssembly = Assembly.Load(reference);
                    foreach (var type in refAssembly.GetTypes())
                    {
                        allTypes.Add(type);
                    }
                }
            }

            var results = new List<FuncTestResult>();
            var index = 0;
            foreach (var test in tests)
            {
                var result = false;
                string error = null;
                try
                {
                    result = test(allTypes);
                    if (!result)
                    {
                        error = ""Test failed."";
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }

                results.Add(new FuncTestResult { Passed = result, Error = error, Index = index });
                index++;
            }

            var jsonResult = JsonTestsResult
                .Replace(""#totalPasses#"", results.Count(t => t.Passed).ToString())
                .Replace(""#passingTests#"", string.Join("","", results.Where(t => t.Passed).Select(t => t.Index).ToList()))
                .Replace(""#message#"", string.Join("", "", results.Where(t => t.Error != null).Select(t => t.Error.Replace(""\"""", string.Empty)).ToList()));

            Console.WriteLine(jsonResult);
        }
    }
}
";

        private const string TestTemplate = @"(types) => { #testInput# }";

        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public CSharpTestRunnerExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc)
        {
            this.getCompilerPathFunc = getCompilerPathFunc;
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var solution = executionContext.FileContent;
            var result = new ExecutionResult();
            var compileResult = this.ExecuteCompiling(executionContext, this.getCompilerPathFunc, result);
            if (!compileResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var outputAssemblyPath = this.PreprocessAndCompileTestRunner(executionContext, compileResult.OutputFile);

            IExecutor executor = new RestrictedProcessExecutor();
            var processExecutionResult = executor.Execute(outputAssemblyPath, string.Empty, executionContext.TimeLimit, executionContext.MemoryLimit);

            var workingDirectory = compileResult.OutputFile;
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, true);
            }

            this.ProcessTests(processExecutionResult, executionContext, result);
            return result;
        }

        private string PreprocessAndCompileTestRunner(ExecutionContext executionContext, string outputDirectory)
        {
            var testStrings = new List<string>();
            foreach (var test in executionContext.Tests)
            {
                testStrings.Add(TestTemplate.Replace("#testInput#", test.Input));
            }

            var allTests = string.Join(",", testStrings);

            var testRunnerCode = TestRunnerTemplate.Replace("#allTests#", allTests);
            var compiler = new CSharpCodeProvider();
            var compilerParameters = new CompilerParameters();
            compilerParameters.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
            compilerParameters.ReferencedAssemblies.AddRange(Directory.GetFiles(outputDirectory).Where(f => f.EndsWith(".dll") || f.EndsWith(".exe")).ToArray());
            compilerParameters.GenerateInMemory = false;
            compilerParameters.GenerateExecutable = true;
            var compilerResult = compiler.CompileAssemblyFromSource(compilerParameters, testRunnerCode);

            var outputAssemblyPath = outputDirectory + "\\LocalTestRunner.exe";
            File.Move(compilerResult.PathToAssembly, outputAssemblyPath);

            return outputAssemblyPath;
        }

        private void ProcessTests(ProcessExecutionResult processExecutionResult, ExecutionContext executionContext, ExecutionResult result)
        {
            var jsonResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput, true, true);

            var index = 0;
            result.TestResults = new List<TestResult>();
            foreach (var test in executionContext.Tests)
            {
                var testResult = new TestResult
                {
                    Id = test.Id,
                    TimeUsed = (int)processExecutionResult.TimeWorked.TotalMilliseconds,
                    MemoryUsed = (int)processExecutionResult.MemoryUsed,
                };

                if (jsonResult.PassingIndexes.Contains(index))
                {
                    testResult.ResultType = TestRunResultType.CorrectAnswer;
                }
                else
                {
                    testResult.ResultType = TestRunResultType.WrongAnswer;
                    testResult.CheckerDetails = new CheckerDetails { Comment = "Test failed." };
                }

                result.TestResults.Add(testResult);
                index++;
            }
        }
    }
}
