namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using OJS.Common.Extensions;
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
            string sinonJsDomModulePath,
            string sinonModulePath,
            string sinonChaiModulePath,
            string underscoreModulePath,
            string babelCliModulePath,
            string reactJsxPluginPath,
            string reactModulePath,
            string reactDomModulePath,
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

            if (!File.Exists(sinonJsDomModulePath))
            {
                throw new ArgumentException($"SinonPackaged not found in: {sinonJsDomModulePath}", nameof(sinonJsDomModulePath));
            }

            if (!Directory.Exists(sinonModulePath))
            {
                throw new ArgumentException($"Sinon not found in: {sinonModulePath}", nameof(sinonModulePath));
            }

            if (!Directory.Exists(sinonChaiModulePath))
            {
                throw new ArgumentException($"Sinon-chai not found in: {sinonChaiModulePath}", nameof(sinonChaiModulePath));
            }

            if (!File.Exists(babelCliModulePath))
            {
                throw new ArgumentException($"Babel-Cli not found in: {babelCliModulePath}", nameof(babelCliModulePath));
            }

            if (!Directory.Exists(reactJsxPluginPath))
            {
                throw new ArgumentException($"React JSX Plugin not found in: {reactJsxPluginPath}", nameof(reactJsxPluginPath));
            }

            if (!File.Exists(reactModulePath))
            {
                throw new ArgumentException($"React Module not found in: {reactModulePath}", nameof(reactModulePath));
            }

            if (!File.Exists(reactDomModulePath))
            {
                throw new ArgumentException($"ReactDOM Module not found in: {reactDomModulePath}", nameof(reactDomModulePath));
            }

            this.JsDomModulePath = this.ProcessModulePath(jsdomModulePath);
            this.JQueryModulePath = this.ProcessModulePath(jqueryModulePath);
            this.HandlebarsModulePath = this.ProcessModulePath(handlebarsModulePath);
            this.SinonJsDomModulePath = this.ProcessModulePath(sinonJsDomModulePath);
            this.SinonModulePath = this.ProcessModulePath(sinonModulePath);
            this.SinonChaiModulePath = this.ProcessModulePath(sinonChaiModulePath);
            this.BabelCliModulePath = this.ProcessModulePath(babelCliModulePath);
            this.ReactJsxPluginPath = this.ProcessModulePath(reactJsxPluginPath);
            this.ReactModulePath = this.ProcessModulePath(reactModulePath);
            this.ReactDomModulePath = this.ProcessModulePath(reactDomModulePath);
        }

        protected string JsDomModulePath { get; }

        protected string JQueryModulePath { get; }

        protected string HandlebarsModulePath { get; }

        protected string SinonModulePath { get; }

        protected string SinonJsDomModulePath { get; }

        protected string SinonChaiModulePath { get; }

        protected string BabelCliModulePath { get; }

        protected string ReactJsxPluginPath { get; }

        protected string ReactModulePath { get; }

        protected string ReactDomModulePath { get; }

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    jsdom = require('" + this.JsDomModulePath + @"'),
    jq = require('" + this.JQueryModulePath + @"'),
    fs = require('fs'),
    sinonJsDom = fs.readFileSync('" + this.SinonJsDomModulePath + @"','utf-8'),
    sinon = require('" + this.SinonModulePath + @"'),
    React = require('" + this.ReactModulePath + @"'),
    ReactDOM = require('" + this.ReactDomModulePath + @"'),
    sinonChai = require('" + this.SinonChaiModulePath + @"'),
    handlebars = require('" + this.HandlebarsModulePath + @"')";

        protected override string JsCodePreevaulationCode => @"
chai.use(sinonChai);
fs = undefined;

describe('TestDOMScope', function() {
    let bgCoderConsole = {};   

    before(function(done) {
        jsdom.env({
            html: '',
            src:[sinonJsDom],
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

    beforeEach(function(){
        global.server = window.sinon.fakeServer.create();
        server.autoRespond = true;
    });

    afterEach(function(){
        server.restore();
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

    let attachPromiseHandlers =(function attachPromiseHandlers(){{
        let result = code.run;
        return function(){{
            let res = result.apply(this,arguments);
            if(res === undefined){{
                isAsyncChecker.isSynchonous = true;
                return;
            }}
            else if(typeof(res.then) === 'function'){{
                return $.when(res);
            }}
            else{{
                isAsyncChecker.isSynchonous = true;
                return res;
            }}
        }}
    }})();

    let isAsyncChecker = {{isSynchonous:false}};
    let testFunc = new Function('isAsyncChecker','result','done', {this.TestFuncVariables}, inputData);
    testFunc.call({{}}, isAsyncChecker , attachPromiseHandlers,done,{this.TestFuncVariables.Replace("'", string.Empty)});
    if(isAsyncChecker.isSynchonous){{
        done();
    }}
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

        protected override string PreprocessJsSubmission(string template, ExecutionContext context, IExecutor executor)
        {
            var code = context.Code.Trim(';');

            var processedCode = template
                .Replace(RequiredModules, this.JsCodeRequiredModules)
                .Replace(PreevaluationPlaceholder, this.JsCodePreevaulationCode)
                .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
                .Replace(TestsPlaceholder, this.BuildTests(context.Tests));

            var tempCode = FileHelpers.SaveStringToTempFile(code);
            var arguments = new List<string>();
            arguments.Add("\"" + this.BabelCliModulePath + "\"");
            arguments.Add("\"" + tempCode + "\"");
            arguments.Add("--plugins");
            arguments.Add("\"" + this.ReactJsxPluginPath + "\"");

            var processExecutionResult = executor.Execute(
                this.NodeJsExecutablePath,
                code,
                context.TimeLimit + this.BaseTimeUsed,
                context.MemoryLimit + this.BaseMemoryUsed,
                arguments);

            processExecutionResult.ReceivedOutput = processExecutionResult.ReceivedOutput.Trim(new[] { '\n', ';' });
            processedCode = processedCode.Replace(UserInputPlaceholder, processExecutionResult.ReceivedOutput);
            File.Delete(tempCode);

            return processedCode;
        }
    }
}
