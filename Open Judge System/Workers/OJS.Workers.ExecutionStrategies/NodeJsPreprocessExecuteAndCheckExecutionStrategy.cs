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
        protected const string LatestEcmaScriptFeaturesEnabledFlag = "--harmony";

        protected const string UserInputPlaceholder = "#userInput#";
        protected const string RequiredModules = "#requiredModule#";
        protected const string PreevaluationPlaceholder = "#preevaluationCode#";
        protected const string PostevaluationPlaceholder = "#postevaluationCode#";
        protected const string EvaluationPlaceholder = "#evaluationCode#";
        protected const string NodeDisablePlaceholder = "#nodeDisableCode";
        protected const string AdapterFunctionPlaceholder = "#adapterFunctionCode#";

        public NodeJsPreprocessExecuteAndCheckExecutionStrategy(
            string nodeJsExecutablePath,
            string underscoreModulePath,
            int baseTimeUsed,
            int baseMemoryUsed)
        {
            if (!File.Exists(nodeJsExecutablePath))
            {
                throw new ArgumentException(
                    $@"NodeJS not found in: {nodeJsExecutablePath}", nameof(nodeJsExecutablePath));
            }

            if (!Directory.Exists(underscoreModulePath))
            {
                throw new ArgumentException(
                    $@"Underscore not found in: {underscoreModulePath}",
                    nameof(underscoreModulePath));
            }

            if (baseTimeUsed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseTimeUsed));
            }

            if (baseMemoryUsed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseMemoryUsed));
            }

            this.NodeJsExecutablePath = nodeJsExecutablePath;
            this.UnderscoreModulePath = this.ProcessModulePath(underscoreModulePath);
            this.BaseTimeUsed = baseTimeUsed;
            this.BaseMemoryUsed = baseMemoryUsed;
        }

        protected string NodeJsExecutablePath { get; }

        protected string UnderscoreModulePath { get; }

        /// <remarks>
        /// Measured in milliseconds.
        /// </remarks>
        protected int BaseTimeUsed { get; set; }

        /// <remarks>
        /// Measured in bytes.
        /// </remarks>
        protected int BaseMemoryUsed { get; set; }

        protected virtual string JsCodeRequiredModules => $@"
var EOL = require('os').EOL,
_ = require('{this.UnderscoreModulePath}')";

        protected virtual string JsNodeDisableCode => @"
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
//process.removeListener = undefined;
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
//setTimeout = undefined;
//clearTimeout = undefined;
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
//delete setTimeout;
//delete clearTimeout;
delete clearInterval;
delete setImmediate;
delete clearImmediate;
delete module;
delete require;
delete msg;

process.exit = function () {};";

        protected virtual string JsCodePreevaulationCode => @"
let content = '';
let code = {
    run: " + UserInputPlaceholder + @"
};
let adapterFunction = " + AdapterFunctionPlaceholder;

        protected virtual string JsCodeEvaluation => @"
process.stdin.resume();
process.stdin.on('data', function(buf) { content += buf.toString(); });
process.stdin.on('end', function() {
    let inputData = content.trim().split(EOL);
    let result = adapterFunction(inputData, code.run);
    if (result !== undefined) {
        console.log(result);
    }
});";

        protected virtual string JsCodePostevaulationCode => string.Empty;

        protected virtual string JsCodeTemplate =>
            RequiredModules + @";" +
            NodeDisablePlaceholder +
            PreevaluationPlaceholder +
            EvaluationPlaceholder +
            PostevaluationPlaceholder;

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            // In NodeJS there is no compilation
            result.IsCompiledSuccessfully = true;

            // Preprocess the user submission
            var codeToExecute = this.PreprocessJsSubmission(
                this.JsCodeTemplate,
                executionContext);

            // Save the preprocessed submission which is ready for execution
            var codeSavePath = FileHelpers.SaveStringToTempFile(codeToExecute);

            // Process the submission and check each test
            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result.TestResults = this.ProcessTests(executionContext, executor, checker, codeSavePath);

            // Clean up
            File.Delete(codeSavePath);

            return result;
        }

        protected virtual List<TestResult> ProcessTests(ExecutionContext executionContext, IExecutor executor, IChecker checker, string codeSavePath)
        {
            var testResults = new List<TestResult>();

            var arguments = new[] { LatestEcmaScriptFeaturesEnabledFlag, codeSavePath };

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = this.ExecuteNodeJsProcess(executionContext, executor, test.Input, arguments);

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                testResults.Add(testResult);
            }

            return testResults;
        }

        protected virtual ProcessExecutionResult ExecuteNodeJsProcess(
            ExecutionContext executionContext,
            IExecutor executor,
            string input,
            IEnumerable<string> additionalArguments)
        {
            var processExecutionResult = executor.Execute(
                this.NodeJsExecutablePath,
                input,
                executionContext.TimeLimit + this.BaseTimeUsed,
                executionContext.MemoryLimit + this.BaseMemoryUsed,
                additionalArguments);

            // No update of time and memory when the result is TimeLimit or MemoryLimit - show how wasteful the solution is
            if (processExecutionResult.Type != ProcessExecutionResultType.TimeLimit &&
                processExecutionResult.Type != ProcessExecutionResultType.MemoryLimit)
            {
                processExecutionResult.TimeWorked = processExecutionResult.TimeWorked.TotalMilliseconds > this.BaseTimeUsed
                    ? processExecutionResult.TimeWorked - TimeSpan.FromMilliseconds(this.BaseTimeUsed)
                    : TimeSpan.FromMilliseconds(this.BaseTimeUsed);
                processExecutionResult.MemoryUsed = Math.Max(processExecutionResult.MemoryUsed - this.BaseMemoryUsed, this.BaseMemoryUsed);
            }

            return processExecutionResult;
        }

        protected string ProcessModulePath(string path) => path.Replace('\\', '/');

        protected virtual string PreprocessJsSubmission(string template, ExecutionContext context)
        {
            var problemSkeleton = context.TaskSkeletonAsString ??
                "function adapter(input, code) { return code(input); }";
            var code = context.Code.Trim(';');

            var processedCode = template
                .Replace(RequiredModules, this.JsCodeRequiredModules)
                .Replace(PreevaluationPlaceholder, this.JsCodePreevaulationCode)
                .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
                .Replace(UserInputPlaceholder, code)
                .Replace(AdapterFunctionPlaceholder, problemSkeleton);

            return processedCode;
        }
    }
}
