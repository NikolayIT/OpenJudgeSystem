namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

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
            this.Random = new Random();
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

        private Random Random { get; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            // In NodeJS there is no compilation
            var result = new ExecutionResult() { IsCompiledSuccessfully = true };

            var executor = new RestrictedProcessExecutor();

            // Preprocess the user submission
            var codeToExecute = this.PreprocessJsSubmission(
                this.JsCodeTemplate,
                executionContext);

            // Save the preprocessed submission which is ready for execution
            var codeSavePath = FileHelpers.SaveStringToTempFile(codeToExecute);

            // Process the submission and check each test
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result.TestResults = this.ProcessTests(executionContext, executor, checker, codeSavePath);

            // Clean up
            File.Delete(codeSavePath);

            return result;
        }

        protected override string BuildTests(IEnumerable<TestContext> tests)
        {
            // Swap the testInput for every copy of the user's tests
            var testGroupRoof = 1;

            // Create a random name for the variable keeping track of the testGroup, so that the user can't manipulate it
            var testGroupVariableName = "testGroup" + this.Random.Next(10000);
            var problemTests = tests.ToList();
            var testsCode = problemTests[0].Input;

            // We set the state of the tested entity in a beforeEach hook to ensure the user doesnt manipulate the entity
            testsCode += @"
let " + testGroupVariableName + $@" = 0;
beforeEach(function(){{
    if(" + testGroupVariableName + $@" < {testGroupRoof}) {{
        {problemTests[1].Input}
    }}";

            testGroupRoof++;
            var beforeHookTests = problemTests.Skip(1).ToList();

            foreach (var test in beforeHookTests)
            {
                testsCode += @"
    else if(" + testGroupVariableName + $@" < {testGroupRoof}) {{
        {test.Input}
    }}";
                testGroupRoof++;
            }

            testsCode += @"
});";

            // Insert a copy of the user tests for each test file
            for (int i = 0; i < problemTests.Count; i++)
            {
                testsCode += $@"
describe('Test {i} ', function(){{
    after(function () {{
        " + testGroupVariableName + $@"++;
    }});
{UserInputPlaceholder}
}});";
            }

            return testsCode;
        }

        protected override List<TestResult> ProcessTests(
            ExecutionContext executionContext,
            IExecutor executor,
            IChecker checker,
            string codeSavePath)
        {
            var testResults = new List<TestResult>();

            var arguments = new List<string>();
            arguments.Add(this.MochaModulePath);
            arguments.Add(codeSavePath);
            arguments.AddRange(executionContext.AdditionalCompilerArguments.Split(' '));

            var testCount = 0;
            var processExecutionResult = this.ExecuteNodeJsProcess(executionContext, executor, string.Empty, arguments);
            var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);
            var numberOfUserTests = mochaResult.UsersTestCount;
            var correctSolutionTestPasses = mochaResult.InitialPassingTests;

            // an offset for tracking the current subset of tests (by convention we always have 2 Zero tests)
            var testOffset = numberOfUserTests * 2;
            foreach (var test in executionContext.Tests)
            {
                var message = "Test Passed!";
                TestResult testResult = null;
                if (testCount == 0)
                {
                    var minTestCount = int.Parse(
                        Regex.Match(
                            test.Input,
                            "<minTestCount>(\\d+)</minTestCount>").Groups[1].Value);
                     if (numberOfUserTests < minTestCount)
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
                    var numberOfPasses = mochaResult.TestsErrors.Skip(testOffset).Take(numberOfUserTests).Count(x => x == null);
                    if (numberOfPasses >= correctSolutionTestPasses)
                    {
                        message = "No unit test covering this functionality!";
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

        protected override string PreprocessJsSubmission(string template, ExecutionContext context)
        {
            var code = context.Code.Trim(';');

            var processedCode =
                template.Replace(RequiredModules, this.JsCodeRequiredModules)
                    .Replace(PreevaluationPlaceholder, this.JsCodePreevaulationCode)
                    .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                    .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                    .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
                    .Replace(TestsPlaceholder, this.BuildTests(context.Tests))
                    .Replace(UserInputPlaceholder, code);

            return processedCode;
        }
    }
}
