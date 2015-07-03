namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Common.Extensions;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class NodeJsPreprocessExecuteAndCheckExecutionStrategy : ExecutionStrategy
    {
        private const string UserInputPlaceholder = "#userInput#";
        private const string RequiredModules = "#requiredModule#";
        private const string PreevaluationPlaceholder = "#preevaluationCode#";
        private const string PostevaluationPlaceholder = "#postevaluationCode#";
        private const string EvaluationPlaceholder = "#evaluationCode#";

        public NodeJsPreprocessExecuteAndCheckExecutionStrategy(string nodeJsExecutablePath)
        {
            if (!File.Exists(nodeJsExecutablePath))
            {
                throw new ArgumentException(string.Format("NodeJS not found in: {0}", nodeJsExecutablePath), "nodeJsExecutablePath");
            }

            this.NodeJsExecutablePath = nodeJsExecutablePath;
        }

        protected string NodeJsExecutablePath { get; private set; }

        protected virtual string JsCodeRequiredModules
        {
            get
            {
                return @"
var EOL = require('os').EOL;";
            }
        }

        protected virtual string JsCodePreevaulationCode
        {
            get
            {
                return @"
var content = ''";
            }
        }

        protected virtual string JsCodePostevaulationCode
        {
            get
            {
                return string.Empty;
            }
        }

        protected virtual string JsCodeEvaluation
        {
            get
            {
                return @"
    var inputData = content.trim().split(EOL);
    var result = code.run(inputData);
    if (result !== undefined) {
        console.log(result);
    }";
            }
        }

        private string JsCodeTemplate
        {
            get
            {
                return RequiredModules + @"

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
process.removeListener = undefined;
process.config = undefined;
// process.on = undefined;
process.openStdin = undefined;
process.chdir = undefined;
process.cwd = undefined;
process.exit = undefined;
process.umask = undefined;
GLOBAL = undefined;
root = undefined;
setTimeout = undefined;
setInterval = undefined;
clearTimeout = undefined;
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
delete GLOBAL;
delete root;
delete setTimeout;
delete setInterval;
delete clearTimeout;
delete clearInterval;
delete setImmediate;
delete clearImmediate;
delete module;
delete require;
delete msg;

process.exit = function () {};

" + PreevaluationPlaceholder + @"
process.stdin.resume();
process.stdin.on('data', function(buf) { content += buf.toString(); });
process.stdin.on('end', function() {
    " + EvaluationPlaceholder + @"
});

" + PostevaluationPlaceholder + @"

var code = {
    run: " + UserInputPlaceholder + @"
};";
            }
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();
            
            // setting the IsCompiledSuccessfully variable to true as in the NodeJS
            // execution strategy there is no compilation
            result.IsCompiledSuccessfully = true;

            // Preprocess the user submission
            var codeToExecute = this.PreprocessJsSubmission(this.JsCodeTemplate, executionContext.Code);

            // Save the preprocessed submission which is ready for execution
            var codeSavePath = FileHelpers.SaveStringToTempFile(codeToExecute);

            // Process the submission and check each test
            IExecutor executor = new RestrictedProcessExecutor();
            IChecker checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);

            result.TestResults = this.ProcessTests(executionContext, executor, checker, codeSavePath);

            // Clean up
            File.Delete(codeSavePath);

            return result;
        }

        protected virtual List<TestResult> ProcessTests(ExecutionContext executionContext, IExecutor executor, IChecker checker, string codeSavePath)
        {
            var testResults = new List<TestResult>();

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(this.NodeJsExecutablePath, test.Input, executionContext.TimeLimit, executionContext.MemoryLimit, new[] { codeSavePath });
                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                testResults.Add(testResult);
            }

            return testResults;
        }

        private string PreprocessJsSubmission(string template, string code)
        {
            var processedCode = template
                .Replace(RequiredModules, this.JsCodeRequiredModules)
                .Replace(PreevaluationPlaceholder, this.JsCodePreevaulationCode)
                .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                .Replace(UserInputPlaceholder, code);

            return processedCode;
        }
    }
}
