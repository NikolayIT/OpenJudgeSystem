namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using MissingFeatures;

    using OJS.Common.Extensions;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy :
        NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy
    {
        public NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy(
            string nodeJsExecutablePath,
            string mochaModulePath,
            string chaiModulePath,
            string jsdomModulePath,
            string jqueryModulePath,
            string handlebarsModulePath,
            string sinonModulePath,
            string sinonChaiModulePath,
            string underscoreModulePath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(
                nodeJsExecutablePath,
                mochaModulePath,
                chaiModulePath,
                jsdomModulePath,
                jqueryModulePath,
                handlebarsModulePath,
                sinonModulePath,
                sinonChaiModulePath,
                underscoreModulePath,
                baseTimeUsed,
                baseMemoryUsed)
        {
        }

        protected override string JsCodePreevaulationCode => @"
chai.use(sinonChai);
let bgCoderConsole = {};
before(function(done)
{
    jsdom.env({
        html: '',
        done: function(errors, window) {
            global.window = window;
            global.document = window.document;
            global.$ = jq(window);
            global.handlebars = handlebars;
            Object.getOwnPropertyNames(window)
                .filter(function(prop) {
                return prop.toLowerCase().indexOf('html') >= 0;
            }).forEach(function(prop) {
                global[prop] = window[prop];
            });

            Object.keys(console)
                .forEach(function (prop) {
                    bgCoderConsole[prop] = console[prop];
                    console[prop] = new Function('');
                });

            done();
        }
    });
});

after(function() {
    Object.keys(bgCoderConsole)
        .forEach(function (prop) {
            console[prop] = bgCoderConsole[prop];
        });
});";

        protected override string JsCodeEvaluation => @"
        " + TestsPlaceholder;

        protected override string JsCodePostevaulationCode => string.Empty;

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            // In NodeJS there is no compilation
            var result = new ExecutionResult() { IsCompiledSuccessfully = true };

            var executor = new RestrictedProcessExecutor();

            // Prerun the zero test in order to get the amount of user tests
            var preRunCode = this.PreprocessPrerunTemplate(
                this.JsCodeTemplate,
                executionContext);
            var prerunCodeSavePath = FileHelpers.SaveStringToTempFile(preRunCode);
            var numberOfUserTests = this.PrerunSubmission(executionContext, executor, prerunCodeSavePath);

            // Preprocess the user submission
            var codeToExecute = this.PreprocessJsSubmission(
                this.JsCodeTemplate,
                executionContext,
                numberOfUserTests);

            // Save the preprocessed submission which is ready for execution
            var codeSavePath = FileHelpers.SaveStringToTempFile(codeToExecute);

            // Process the submission and check each test
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result.TestResults = this.ProcessTests(executionContext, executor, checker, codeSavePath, numberOfUserTests);

            // Clean up
            File.Delete(prerunCodeSavePath);
            File.Delete(codeSavePath);

            return result;
        }

        protected virtual string BuildTests(IEnumerable<TestContext> tests, int numberOfUserTests)
        {
            // Swap the testInput every X amount of tests where X is the amount of User Tests
            // We keep track of the number we must swap at with the variable testCountRoof
            var testCountRoof = numberOfUserTests;
            var problemTests = tests.ToList();
            var testsCode = problemTests[0].Input;

            // We set the state of the tested entity in a beforeEach hook to ensure the user doesnt manipulate the entity
            testsCode += $@"
let testsCount = 0;
beforeEach(function(){{
    if(testsCount < {testCountRoof}) {{
        {problemTests[1].Input}
    }}";

            testCountRoof += numberOfUserTests;
            var beforeHookTests = problemTests.Skip(2).ToList();

            foreach (var test in beforeHookTests)
            {
                testsCode += $@"
    else if(testsCount < {testCountRoof}) {{
        {test.Input}
    }}";
                testCountRoof += numberOfUserTests;
            }

            testsCode += @"
    testsCount++;
});";

            // Insert a copy of the user tests for each test file except the first zero test as it just checks the test count
            for (int i = 1; i <= problemTests.Count - 1; i++)
            {
                testsCode += @"
describe('JudgeTest" + i + @"', function(){
" + UserInputPlaceholder + @"
});";
            }

            return testsCode;
        }

        protected virtual string BuildPrerunTests(IEnumerable<TestContext> tests)
        {
            var testsCode = string.Empty;
            testsCode += $@"
                {tests.FirstOrDefault().Input}
                describe('JudgeTest1', function(){{ 
                {UserInputPlaceholder}
                }});";

            return testsCode;
        }

        protected virtual int PrerunSubmission(
            ExecutionContext executionContext,
            IExecutor executor,
            string codeSavePath)
        {
            var arguments = new List<string>();
            arguments.Add(this.MochaModulePath);
            arguments.Add(codeSavePath);
            arguments.AddRange(executionContext.AdditionalCompilerArguments.Split(' '));

            var processExecutionResult = this.ExecuteNodeJsProcess(executionContext, executor, string.Empty, arguments);
            var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);
            return mochaResult.TotalTests;
        }

        protected virtual List<TestResult> ProcessTests(
            ExecutionContext executionContext,
            IExecutor executor,
            IChecker checker,
            string codeSavePath,
            int numberOfUserTests)
        {
            var testResults = new List<TestResult>();

            var arguments = new List<string>();
            arguments.Add(this.MochaModulePath);
            arguments.Add(codeSavePath);
            arguments.AddRange(executionContext.AdditionalCompilerArguments.Split(' '));

            var testCount = 0;
            var processExecutionResult = this.ExecuteNodeJsProcess(executionContext, executor, string.Empty, arguments);
            var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);
            var correctSolutionTestPasses = mochaResult.TestsErrors.Take(numberOfUserTests).Count(x => x == string.Empty);
            var testOffset = numberOfUserTests;
            foreach (var test in executionContext.Tests)
            {
                TestResult testResult = null;
                if (testCount == 0)
                {
                    var minTestCount = int.Parse(
                        Regex.Match(
                            test.Input,
                            "<minTestCount>(\\d+)</minTestCount>").Groups[1].Value);
                    var message = "yes";
                    if (numberOfUserTests == 0)
                    {
                        message = "The submitted code was either incorrect or contained no tests!";
                    }
                    else if (numberOfUserTests < minTestCount)
                    {
                        message = $"Insufficient amount of tests, you have to have atleast {minTestCount} tests!";
                    }

                    testResult = this.ExecuteAndCheckTest(
                        test,
                        processExecutionResult,
                        checker,
                        message);
                }
                else if (testCount == 1)
                {
                    var message = "yes";
                    if (numberOfUserTests == 0)
                    {
                        message = "The submitted code was either incorrect or contained no tests!";
                    }
                    else if (correctSolutionTestPasses != numberOfUserTests)
                    {
                        message = "Error: Some tests failed while running the correct solution!";
                    }

                    testResult = this.ExecuteAndCheckTest(
                        test,
                        processExecutionResult,
                        checker,
                        message);
                }
                else
                {
                    var numberOfPasses = mochaResult.TestsErrors.Skip(testOffset).Take(numberOfUserTests).Count(x => x == string.Empty);
                    var message = "yes";
                    if (numberOfPasses >= correctSolutionTestPasses)
                    {
                        message = "No unit test covering this functionality!";
                        mochaResult.TestsErrors
                            .Take(numberOfUserTests)
                            .Where(x => x != string.Empty)
                            .ForEach(x => message += x);
                    }

                    testResult = this.ExecuteAndCheckTest(
                        test,
                        processExecutionResult,
                        checker,
                        message);
                    testOffset += numberOfUserTests;
                }

                testCount++;
                testResults.Add(testResult);
            }

            return testResults;
        }

        protected virtual string PreprocessJsSubmission(string template, ExecutionContext context, int numberOfUserTests)
        {
            var code = context.Code.Trim(';');

            var processedCode =
                template.Replace(RequiredModules, this.JsCodeRequiredModules)
                    .Replace(PreevaluationPlaceholder, this.JsCodePreevaulationCode)
                    .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                    .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                    .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
                    .Replace(TestsPlaceholder, this.BuildTests(context.Tests, numberOfUserTests))
                    .Replace(UserInputPlaceholder, code);

            return processedCode;
        }

        protected virtual string PreprocessPrerunTemplate(string template, ExecutionContext context)
        {
            var code = context.Code.Trim(';');

            var processedCode =
                template.Replace(RequiredModules, this.JsCodeRequiredModules)
                    .Replace(PreevaluationPlaceholder, this.JsCodePreevaulationCode)
                    .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                    .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                    .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
                    .Replace(TestsPlaceholder, this.BuildPrerunTests(context.Tests))
                    .Replace(UserInputPlaceholder, code);
            return processedCode;
        }
    }
}
