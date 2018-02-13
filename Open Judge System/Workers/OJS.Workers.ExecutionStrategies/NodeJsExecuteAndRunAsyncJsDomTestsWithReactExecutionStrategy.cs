namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using OJS.Common.Extensions;
    using OJS.Workers.Common;

    public class NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy :
        NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy
    {
        public NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy(
            string nodeJsExecutablePath,
            string mochaModulePath,
            string chaiModulePath,
            string jsdomModulePath,
            string jqueryModulePath,
            string handlebarsModulePath,
            string sinonJsDomModulePath,
            string sinonModulePath,
            string sinonChaiModulePath,
            string underscoreModulePath,
            string babelCoreModulePath,
            string reactJsxPluginPath,
            string reactModulePath,
            string reactDomModulePath,
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
            if (!File.Exists(sinonJsDomModulePath))
            {
                throw new ArgumentException(
                    $"SinonPackaged not found in: {sinonJsDomModulePath}",
                    nameof(sinonJsDomModulePath));
            }

            if (!Directory.Exists(babelCoreModulePath))
            {
                throw new ArgumentException(
                    $"Babel-Core not found in: {babelCoreModulePath}",
                    nameof(babelCoreModulePath));
            }

            if (!Directory.Exists(reactJsxPluginPath))
            {
                throw new ArgumentException(
                    $"React JSX Plugin not found in: {reactJsxPluginPath}",
                    nameof(reactJsxPluginPath));
            }

            if (!Directory.Exists(reactModulePath))
            {
                throw new ArgumentException(
                    $"React Module not found in: {reactModulePath}",
                    nameof(reactModulePath));
            }

            if (!Directory.Exists(reactDomModulePath))
            {
                throw new ArgumentException(
                    $"ReactDOM Module not found in: {reactDomModulePath}",
                    nameof(reactDomModulePath));
            }

            this.SinonJsDomModulePath = FileHelpers.ProcessModulePath(sinonJsDomModulePath);
            this.BabelCoreModulePath = FileHelpers.ProcessModulePath(babelCoreModulePath);
            this.ReactJsxPluginPath = FileHelpers.ProcessModulePath(reactJsxPluginPath);
            this.ReactModulePath = FileHelpers.ProcessModulePath(reactModulePath);
            this.ReactDomModulePath = FileHelpers.ProcessModulePath(reactDomModulePath);
        }

        protected string SinonJsDomModulePath { get; }

        protected string BabelCoreModulePath { get; }

        protected string ReactJsxPluginPath { get; }

        protected string ReactModulePath { get; }

        protected string ReactDomModulePath { get; }

        protected override string JsCodeTemplate =>
    RequiredModules + $@";
    let code =  `{UserInputPlaceholder}`;

    code = babel.transform(code, {{plugins: [reactJsxPlugin]}});
    code = code.code;
    code = `let result = ${{code}}\n`;" +
    NodeDisablePlaceholder +
    PreevaluationPlaceholder +
    EvaluationPlaceholder +
    PostevaluationPlaceholder;

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    fs = require('fs'),
    sinonJsDom = fs.readFileSync('" + this.SinonJsDomModulePath + @"','utf-8'),
    React = require('" + this.ReactModulePath + @"'),
    ReactDOM = require('" + this.ReactDomModulePath + @"'),
    babel = require('" + this.BabelCoreModulePath + @"'),
    reactJsxPlugin = require('" + this.ReactJsxPluginPath + @"')";

        protected override string JsNodeDisableCode => base.JsNodeDisableCode + @"
fs = undefined;";

        protected override string JsCodePreevaulationCode => $@"
chai.use(sinonChai);

describe('TestDOMScope', function() {{
    let bgCoderConsole = {{}};   

    before(function(done) {{
        jsdom.env({{
            html: '',
            src:[sinonJsDom],
            done: function(errors, window) {{
                global.window = window;
                global.document = window.document;
                global.$ = jq(window);
                global.handlebars = handlebars;
                Object.getOwnPropertyNames(window)
                    .filter(function (prop) {{
                        return prop.toLowerCase().indexOf('html') >= 0;
                    }}).forEach(function (prop) {{
                        global[prop] = window[prop];
                    }});

                Object.keys(console)
                    .forEach(function (prop) {{
                        bgCoderConsole[prop] = console[prop];
                        console[prop] = new Function('');
                    }});

                done();
            }}
        }});
    }});

    beforeEach(function(){{
        window.XMLHttpRequest = window.sinon.useFakeXMLHttpRequest();
        global.server = window.sinon.fakeServer.create();
        server.autoRespond = true;
    }});

    afterEach(function(){{
        server.restore();
    }});

    after(function() {{
        Object.keys(bgCoderConsole)
            .forEach(function (prop) {{
                console[prop] = bgCoderConsole[prop];
            }});
    }});";

        protected override string TestFuncVariables => base.TestFuncVariables + ", 'React', 'ReactDOM'";

        protected override string BuildTests(IEnumerable<TestContext> tests)
        {
            var testsCode = string.Empty;
            var testsCount = 1;
            foreach (var test in tests)
            {
                var code = Regex.Replace(test.Input, "([\\\\`$])", "\\$1");

                testsCode +=
                    $@"
it('Test{testsCount++}', function(done) {{
    let content = `{code}`;
    let inputData = content.trim();

    let testFunc = new Function('done', {this.TestFuncVariables}, code + inputData);
    testFunc.call({{}}, done, {this.TestFuncVariables.Replace("'", string.Empty)});
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

            var processExecutionResult = executor.Execute(
                this.NodeJsExecutablePath,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                arguments);

            var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);
            var currentTest = 0;

            foreach (var test in executionContext.Tests)
            {
                var message = "yes";
                if (!string.IsNullOrEmpty(mochaResult.Error))
                {
                    message = mochaResult.Error;
                }
                else if (mochaResult.TestErrors[currentTest] != null)
                {
                    message = $"Unexpected error: {mochaResult.TestErrors[currentTest]}";
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
            code = Regex.Replace(code, "([\\\\`$])", "\\$1");

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
