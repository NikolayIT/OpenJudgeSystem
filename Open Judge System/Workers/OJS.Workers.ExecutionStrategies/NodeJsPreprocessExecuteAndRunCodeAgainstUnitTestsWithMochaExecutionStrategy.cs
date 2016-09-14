namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using OJS.Workers.Common;

    public class NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy :
        IoJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy
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
                nodeJsExecutablePath, mochaModulePath, chaiModulePath, jsdomModulePath, jqueryModulePath,
                handlebarsModulePath, sinonModulePath, sinonChaiModulePath, underscoreModulePath, baseTimeUsed,
                baseMemoryUsed)
        {
            if (!File.Exists(mochaModulePath))
            {
                throw new ArgumentException($"Mocha not found in: {mochaModulePath}", nameof(mochaModulePath));
            }

            this.MochaModulePath = this.ProcessModulePath(mochaModulePath);
        }

        protected string MochaModulePath { get; }

        // TODO Maybe create a flexible base template
        protected override string JsCodeTemplate => RequiredModules + @";
DataView = undefined;
DTRACE_NET_SERVER_CONNECTION = undefined;
// DTRACE_NET_STREAM_END = undefined;
DTRACE_NET_SOCKET_READ = undefined;
DTRACE_NET_SOCKET_WRITE = undefined;
DTRACE_HTTP_SERVER_REQUEST = undefined;
DTRACE_HTTP_SERVER_RESPONSE = undefined;
DTRACE_HTTP_CLIENT_REQUEST = undefined;
DTRACE_HTTP_CLIENT_RESPONSE = undefined;
COUNTER_NET_SERVER_CONNECTION = undefined;
COUNTER_NET_SERVER_CONNECTION_CLOSE = undefined;
COUNTER_HTTP_SERVER_REQUEST = undefined;
COUNTER_HTTP_SERVER_RESPONSE = undefined;
COUNTER_HTTP_CLIENT_REQUEST = undefined;
COUNTER_HTTP_CLIENT_RESPONSE = undefined;
process.argv = undefined;
process.versions = undefined;
process.env = { NODE_DEBUG: false };
process.addListener = undefined;
process.EventEmitter = undefined;
process.mainModule = undefined;
process.config = undefined;
// process.on = undefined;
process.openStdin = undefined;
process.chdir = undefined;
process.cwd = undefined;
process.exit = undefined;
process.umask = undefined;
// GLOBAL = undefined;
// root = undefined;
// global = {};
setInterval = undefined;
// clearTimeout = undefined;
clearInterval = undefined;
setImmediate = undefined;
clearImmediate = undefined;
module = undefined;
require = undefined;
msg = undefined;

delete DataView;
delete DTRACE_NET_SERVER_CONNECTION;
// delete DTRACE_NET_STREAM_END;
delete DTRACE_NET_SOCKET_READ;
delete DTRACE_NET_SOCKET_WRITE;
delete DTRACE_HTTP_SERVER_REQUEST;
delete DTRACE_HTTP_SERVER_RESPONSE;
delete DTRACE_HTTP_CLIENT_REQUEST;
delete DTRACE_HTTP_CLIENT_RESPONSE;
delete COUNTER_NET_SERVER_CONNECTION;
delete COUNTER_NET_SERVER_CONNECTION_CLOSE;
delete COUNTER_HTTP_SERVER_REQUEST;
delete COUNTER_HTTP_SERVER_RESPONSE;
delete COUNTER_HTTP_CLIENT_REQUEST;
delete COUNTER_HTTP_CLIENT_RESPONSE;
delete process.argv;
delete process.exit;
delete process.versions;
// delete GLOBAL;
// delete root;
delete setInterval;
// delete clearTimeout;
delete clearInterval;
delete setImmediate;
delete clearImmediate;
delete module;
delete require;
delete msg;

process.exit = function () {};
var code = {
    run: " + UserInputPlaceholder + @"
};
" + PreevaluationPlaceholder + @"
" + EvaluationPlaceholder + @"
" + PostevaluationPlaceholder;

        protected override string JsCodePreevaulationCode => @"
chai.use(sinonChai);

describe('TestDOMScope', function() {

process.nextTick(function() {
    content = '';
    jsdom.env({
                html: '',
                done: function(errors, window) {
                    global.window = window;
                    global.document = window.document;
                    global.$ = jq(window);
                    global.handlebars = handlebars;
                    Object.keys(window)
                        .filter(function (prop) {
                            return prop.toLowerCase().indexOf('html') >= 0;
                        }).forEach(function (prop) {
                            global[prop] = window[prop];
                        });

                    process.stdin.resume();
                    process.stdin.on('data', function(buf) { content += buf.toString(); });
                    process.stdin.on('end', function(){
                            defineTests(content);
                            run();
                    });
                }
            });
    });";
        
        protected override string JsCodeEvaluation => @"
    function defineTests(content){

        describe('Tests',function(){
            var bgCoderConsole = {};            
            Object.keys(console)
                .forEach(function (prop) {
                    bgCoderConsole[prop] = console[prop];
                    console[prop] = new Function('');
                });
            var inputData = content.trim();
            var codeInput = code.run;
            var executingCode = '\'use strict\';';
            executingCode += inputData + codeInput;

            var testFunc = new Function(" + this.TestFuncVariables + @", executingCode);
            testFunc.call({}, " + this.TestFuncVariables.Replace("'", string.Empty) + @");
            
            Object.keys(bgCoderConsole)
                .forEach(function (prop) {
                console[prop] = bgCoderConsole[prop];
            });
        });
    }";

        protected override string JsCodePostevaulationCode => @"
});";

        protected override string TestFuncVariables => base.TestFuncVariables + @",'describe','it','before','beforeEach','after','afterEach'";

        protected override List<TestResult> ProcessTests(ExecutionContext executionContext, IExecutor executor, IChecker checker, string codeSavePath)
        {
            var testResults = new List<TestResult>();

            var arguments = new List<string>();
            arguments.Add(this.MochaModulePath);
            arguments.Add(codeSavePath);
            arguments.AddRange(executionContext.AdditionalCompilerArguments.Split(' '));

            var initialPassedTests = 0;
            var testCount = 0;
            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = this.ExecuteNodeJsProcess(executionContext, executor, test.Input, arguments);
                var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput, true);
                TestResult testResult = null;
                if (testCount == 0)
                {
                    var minTestCount = int.Parse(Regex.Match(test.Input, "<minTestCount>(\\d+)</minTestCount>").Groups[1].Value);
                    var result = "yes";
                    if (mochaResult.TotalTests == 0)
                    {
                        result = "The submitted code was either incorrect or contained no tests!";             
                    }
                    else if (mochaResult.TotalTests != 0 && mochaResult.TotalTests < minTestCount)
                    {
                        result = $"Insufficient amount of tests, you have to have atleast {minTestCount} tests!";
                    }

                    testResult = this.ExecuteAndCheckTest(
                        test,
                        processExecutionResult,
                        checker,
                        result);
                }
                else if (testCount == 1)
                {
                    var result = "yes";
                    if (mochaResult.TotalTests == 0)
                    {
                        result = "The submitted code was either incorrect or contained no tests!";
                    }
                    else if (mochaResult.TotalTests != 0 && mochaResult.TotalTests != mochaResult.TotalPasses)
                    {
                        result = "Error: Some tests failed while running the correct solution!";
                    }

                    testResult = this.ExecuteAndCheckTest(
                        test,
                        processExecutionResult,
                        checker,
                        result);
                    initialPassedTests = mochaResult.TotalPasses;
                }
                else
                {
                    var result = "No unit test covering this functionality!";
                    if (mochaResult.TotalPasses < initialPassedTests)
                    {
                        result = "yes";
                    }
                    else if (mochaResult.Error != null)
                    {
                        result = $"Unexpected error:{mochaResult.Error}";
                    }

                    testResult = this.ExecuteAndCheckTest(
                        test,
                        processExecutionResult,
                        checker,
                        result);
                }

                testCount++;
                testResults.Add(testResult);
            }

            return testResults;
        }

        protected override string PreprocessJsSubmission(string template, string code)
        {
            code = Regex.Replace(code, "([\"'])", "\\$1");
            code = Regex.Replace(code, "[\r\n\t]*", string.Empty);
            return base.PreprocessJsSubmission(template, "'" + code + "'");
        }
    }
}
