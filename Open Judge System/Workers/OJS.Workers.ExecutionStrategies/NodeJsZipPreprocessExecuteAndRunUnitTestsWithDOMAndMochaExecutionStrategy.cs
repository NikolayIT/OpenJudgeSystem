namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Checkers;
    using Common;
    using Executors;
    using Ionic.Zip;
    using OJS.Common;
    using OJS.Common.Extensions;

    public class NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMochaExecutionStrategy :
        IoJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy
    {
        protected const string AppJsFileName = "app.js";
        protected const string SubmissionFileName = "_$submission";

        public NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMochaExecutionStrategy(
            string nodeJsExecutablePath,
            string mochaModulePath,
            string chaiModulePath,
            string jsdomModulePath,
            string jqueryModulePath,
            string handlebarsModulePath,
            string sinonModulePath,
            string sinonChaiModulePath,
            string underscoreModulePath,
            string browserifyModulePath,
            string babelifyModulePath,
            string ecmaScriptImportPluginPath,
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
            if (!Directory.Exists(browserifyModulePath))
            {
                throw new ArgumentException(
                    $"Browsrify not found in: {browserifyModulePath}",
                    nameof(browserifyModulePath));
            }

            if (!Directory.Exists(babelifyModulePath))
            {
                throw new ArgumentException(
                    $"Babel not found in: {babelifyModulePath}",
                    nameof(babelifyModulePath));
            }

            if (!Directory.Exists(ecmaScriptImportPluginPath))
            {
                throw new ArgumentException(
                    $"ECMAScript2015ImportPluginPath not found in: {ecmaScriptImportPluginPath}",
                    nameof(ecmaScriptImportPluginPath));
            }

            this.BrowserifyModulePath = this.ProcessModulePath(browserifyModulePath);
            this.BabelifyModulePath = this.ProcessModulePath(babelifyModulePath);
            this.EcmaScriptImportPluginPath = this.ProcessModulePath(ecmaScriptImportPluginPath);
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectory();
        }

        ~NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMochaExecutionStrategy()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
        }

        protected string BrowserifyModulePath { get; }

        protected string BabelifyModulePath { get; }

        protected string EcmaScriptImportPluginPath { get; }

        protected string WorkingDirectory { get; set; }

        protected string ProgramEntryPath { get; set; }

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    browserify = require('" + this.BrowserifyModulePath + @"'),
    streamJs = require('stream'),
    stream = new streamJs.PassThrough();";

        protected string JsNodeDisableCode => @"
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

process.exit = function () {};";

        protected override string JsCodeTemplate =>
            this.JsCodeRequiredModules +
            this.JsCodePreevaulationCode +
            this.JsCodeEvaluation +
            this.JsCodePostevaulationCode;

        protected override string JsCodePreevaulationCode => @"
chai.use(sinonChai);

describe('TestDOMScope', function() {
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
                done();
            }
        });
    });

	it('Test', function(done) {
        this.timeout(30000);
		var content = '';
        var userBundleCode = '';
            process.stdin.resume();
            process.stdin.on('data', function (buf) {
                content += buf.toString();
            });
            process.stdin.on('end', function () {
                stream.on('data', function (x) {
                    userBundleCode += x;
                });
                stream.on('end', afterBundling);
                browserify(" + UserInputPlaceholder + @")
                    .transform('" + this.BabelifyModulePath + @"', { plugins: ['" + this.EcmaScriptImportPluginPath + @"']})
                    .bundle()
                    .pipe(stream);";

        protected override string JsCodeEvaluation => @"
        function afterBundling()
        {
            var bgCoderConsole = {};
            Object.keys(console)
                .forEach(function (prop) {
                    bgCoderConsole[prop] = console[prop];
                    console[prop] = new Function('');
                });"

            + this.JsNodeDisableCode + @"

            var testCode = content.trim();
            var testFunc = new Function(" + this.TestFuncVariables + @",'code', testCode);
            var result = testFunc.call({}," + this.TestFuncVariables.Replace("'", string.Empty) + @", userBundleCode);

            Object.keys(bgCoderConsole)
                .forEach(function (prop) {
                    console[prop] = bgCoderConsole[prop];
                });
            done();
        }";

        protected override string JsCodePostevaulationCode => @"
        });
    });
});";

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult { IsCompiledSuccessfully = true };

            // Copy and unzip the file (save file to WorkingDirectory)
            this.CreateSubmissionFile(executionContext);
            this.ProgramEntryPath = this.FindProgramEntryPath();

            // Replace the placeholders in the JS Template with the real values
            var codeToExecute = this.PreprocessJsSubmission(
                this.JsCodeTemplate,
                this.ProgramEntryPath,
                executionContext.TaskSkeletonAsString);

            // Save code to file
            var codeSavePath = FileHelpers.SaveStringToTempFile(codeToExecute);

            // Create a Restricted Process Executor
            var executor = new RestrictedProcessExecutor();

            // Create a Checker using the information from the Execution Context
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            // Process tests 
            result.TestResults = this.ProcessTests(executionContext, executor, checker, codeSavePath);

            // Clean up
            File.Delete(codeSavePath);

            return result;
        }

        protected virtual string CreateSubmissionFile(ExecutionContext executionContext)
        {
            var trimmedAllowedFileExtensions = executionContext.AllowedFileExtensions?.Trim();

            var allowedFileExtensions = (!trimmedAllowedFileExtensions?.StartsWith(".") ?? false)
                ? $".{trimmedAllowedFileExtensions}"
                : trimmedAllowedFileExtensions;

            if (allowedFileExtensions != GlobalConstants.ZipFileExtension)
            {
                throw new ArgumentException("Submission file is not a zip file!");
            }

            return this.PrepareSubmissionFile(executionContext.FileContent);
        }

        protected virtual string PrepareSubmissionFile(byte[] submissionFileContent)
        {
            var submissionFilePath = $"{this.WorkingDirectory}\\{SubmissionFileName}";

            File.WriteAllBytes(submissionFilePath, submissionFileContent);

            this.ConvertContentToZip(submissionFilePath);

            this.UnzipFile(submissionFilePath, this.WorkingDirectory);

            return submissionFilePath;
        }

        protected virtual void ConvertContentToZip(string submissionZipFilePath)
        {
            using (var zipFile = new ZipFile(submissionZipFilePath))
            {
                zipFile.Save();
            }
        }

        protected virtual void UnzipFile(string fileToUnzip, string outputDirectory)
        {
            using (var zipFile = ZipFile.Read(fileToUnzip))
            {
                foreach (var entry in zipFile)
                {
                    entry.Extract(outputDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
            }

            File.Delete(fileToUnzip);
        }

        protected virtual string FindProgramEntryPath()
        {
            var files = new List<string>(
                Directory.GetFiles(
                    this.WorkingDirectory,
                    "app.js",
                    SearchOption.AllDirectories));
            if (files.Count == 0)
            {
                throw new ArgumentException(
                    $"'{AppJsFileName}' file not found in output directory!",
                    nameof(this.WorkingDirectory));
            }

            return this.ProcessModulePath("\"" + files[0] + "\"");
        }
    }
}
