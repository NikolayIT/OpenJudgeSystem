namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using OJS.Workers.Common;

    public class NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy
    {
        public NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
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
            int baseMemoryUsed) // TODO: make this modular by getting requires from test
            : base(
                nodeJsExecutablePath,
                mochaModulePath,
                chaiModulePath,
                underscoreModulePath,
                baseTimeUsed,
                baseMemoryUsed)
        {
            if (!Directory.Exists(jsdomModulePath))
            {
                throw new ArgumentException($"jsDom not found in: {jsdomModulePath}", nameof(jsdomModulePath));
            }

            if (!Directory.Exists(jqueryModulePath))
            {
                throw new ArgumentException($"jQuery not found in: {jqueryModulePath}", nameof(jqueryModulePath));
            }

            if (!Directory.Exists(handlebarsModulePath))
            {
                throw new ArgumentException($"Handlebars not found in: {handlebarsModulePath}", nameof(handlebarsModulePath));
            }

            if (!Directory.Exists(sinonModulePath))
            {
                throw new ArgumentException($"Sinon not found in: {sinonModulePath}", nameof(sinonModulePath));
            }

            if (!Directory.Exists(sinonChaiModulePath))
            {
                throw new ArgumentException($"Sinon-chai not found in: {sinonChaiModulePath}", nameof(sinonChaiModulePath));
            }

            this.JsDomModulePath = this.ProcessModulePath(jsdomModulePath);
            this.JQueryModulePath = this.ProcessModulePath(jqueryModulePath);
            this.HandlebarsModulePath = this.ProcessModulePath(handlebarsModulePath);
            this.SinonModulePath = this.ProcessModulePath(sinonModulePath);
            this.SinonChaiModulePath = this.ProcessModulePath(sinonChaiModulePath);
        }

        protected string JsDomModulePath { get; }

        protected string JQueryModulePath { get; }

        protected string HandlebarsModulePath { get; }

        protected string SinonModulePath { get; }

        protected string SinonChaiModulePath { get; }

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    jsdom = require('" + this.JsDomModulePath + @"'),
    jq = require('" + this.JQueryModulePath + @"'),
    sinon = require('" + this.SinonModulePath + @"'),
    sinonChai = require('" + this.SinonChaiModulePath + @"'),
    handlebars = require('" + this.HandlebarsModulePath + @"')";

        protected override string JsCodePreevaulationCode => @"
chai.use(sinonChai);
describe('TestDOMScope', function() {
    let bgCoderConsole = {};   
    before(function(done) {
        jsdom.env({
            html: '',
            done: function(errors, window) {
                global.window = window;
                global.document = window.document;
                global.$ = jq(window);
                global.handlebars = handlebars;
                Object.getOwnPropertyNames(window)
                    .filter(function (prop) {
                        return prop.toLowerCase().indexOf('html') >= 0;
                    }).forEach(function (prop) {
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

        protected override string JsCodeEvaluation => TestsPlaceholder;

        protected override string TestFuncVariables => base.TestFuncVariables + ", '_', 'sinon'";

        protected virtual string BuildTests(IEnumerable<TestContext> tests)
        {
            var testsCode = string.Empty;
            var testsCount = 1;
            foreach (var test in tests)
            {
                var code = Regex.Replace(test.Input, "([\\\\`])", "\\$1");

                testsCode +=
                    $@"
it('Test{testsCount++}', function(done) {{
    let content = `{code}`;
    let inputData = content.trim();
    let code = {{
        run: {UserInputPlaceholder}
    }};
    let testFunc = new Function('result', {this.TestFuncVariables}, inputData);
    testFunc.call({{}}, code.run, {this.TestFuncVariables.Replace("'", string.Empty)});
    done();
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
            var processExecutionResult = this.ExecuteNodeJsProcess(executionContext, executor, string.Empty, arguments);
            var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);
            var currentTest = 0;
            foreach (var test in executionContext.Tests)
            {
                var message = "yes";
                if (!string.IsNullOrEmpty(mochaResult.Error))
                {
                    message = mochaResult.Error;
                }
                else if (mochaResult.TestsErrors[currentTest] != null)
                {
                    message = $"Unexpected error: {mochaResult.TestsErrors[currentTest]}";
                }

                var testResult = this.ExecuteAndCheckTest(
                    test,
                    processExecutionResult,
                    checker,
                    message);
                currentTest++;
                testResults.Add(testResult);
            }

            return testResults;
        }

        protected override string PreprocessJsSubmission(string template, ExecutionContext context)
        {
            var code = context.Code.Trim(';');
            var processedCode = template
                .Replace(RequiredModules, this.JsCodeRequiredModules)
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