namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Executors;

    public class NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMochaExecutionStrategy :
        NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy
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

            this.BrowserifyModulePath = FileHelpers.ProcessModulePath(browserifyModulePath);
            this.BabelifyModulePath = FileHelpers.ProcessModulePath(babelifyModulePath);
            this.EcmaScriptImportPluginPath = FileHelpers.ProcessModulePath(ecmaScriptImportPluginPath);
        }

        protected string BrowserifyModulePath { get; }

        protected string BabelifyModulePath { get; }

        protected string EcmaScriptImportPluginPath { get; }

        protected string ProgramEntryPath { get; set; }

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    browserify = require('" + this.BrowserifyModulePath + @"'),
    streamJs = require('stream'),
    stream = new streamJs.PassThrough();";

        protected override string JsCodeTemplate =>
            RequiredModules + @";" +
            PreevaluationPlaceholder +
            EvaluationPlaceholder +
            PostevaluationPlaceholder;

        protected override string JsCodePreevaulationCode => @"
chai.use(sinonChai);
let userBundleCode = '';
stream.on('data', function (x) {
    userBundleCode += x;
});
stream.on('end', function(){
    afterBundling(userBundleCode);
    run();
});
browserify('" + UserInputPlaceholder + @"')
    .transform('" + this.BabelifyModulePath + @"', { plugins: ['" + this.EcmaScriptImportPluginPath + @"']})
    .bundle()
    .pipe(stream);

function afterBundling() {
    describe('TestDOMScope', function() {
    let bgCoderConsole = {};
        before(function(done) {" +
            NodeDisablePlaceholder + @"
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

        protected override string JsCodeEvaluation => @"
            " + TestsPlaceholder;

        protected override string JsCodePostevaulationCode => @"
    });
}";

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult { IsCompiledSuccessfully = true };

            // Copy and unzip the file (save file to WorkingDirectory)
            this.CreateSubmissionFile(executionContext);
            this.ProgramEntryPath = FileHelpers.FindFileMatchingPattern(this.WorkingDirectory, AppJsFileName);

            // Replace the placeholders in the JS Template with the real values
            var codeToExecute = this.PreprocessJsSubmission(
                this.JsCodeTemplate,
                executionContext,
                this.ProgramEntryPath);

            // Save code to file
            var codeSavePath = FileHelpers.SaveStringToTempFile(this.WorkingDirectory, codeToExecute);

            // Create a Restricted Process Executor
            var executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);

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

        protected override string BuildTests(IEnumerable<TestContext> tests)
        {
            var testsCode = string.Empty;
            var testsCount = 1;
            foreach (var test in tests)
            {
                var code = Regex.Replace(test.Input, "([\\\\`$])", "\\$1");

                testsCode += $@"
                it('Test{testsCount++}', function(done) {{
                    this.timeout(10000);
            	    let content = `{code}`;

                    let testFunc = new Function({this.TestFuncVariables},'code', content);
                    testFunc.call({{}},{this.TestFuncVariables.Replace("'", string.Empty)}, userBundleCode);

                    done();
                }});";
            }

            return testsCode;
        }

        protected virtual string CreateSubmissionFile(ExecutionContext executionContext)
        {
            var trimmedAllowedFileExtensions = executionContext.AllowedFileExtensions?.Trim();

            var allowedFileExtensions = (!trimmedAllowedFileExtensions?.StartsWith(".") ?? false)
                ? $".{trimmedAllowedFileExtensions}"
                : trimmedAllowedFileExtensions;

            if (allowedFileExtensions != Constants.ZipFileExtension)
            {
                throw new ArgumentException("Submission file is not a zip file!");
            }

            return this.PrepareSubmissionFile(executionContext.FileContent);
        }

        protected virtual string PrepareSubmissionFile(byte[] submissionFileContent)
        {
            var submissionFilePath = $"{this.WorkingDirectory}\\{SubmissionFileName}";
            File.WriteAllBytes(submissionFilePath, submissionFileContent);
            FileHelpers.ConvertContentToZip(submissionFilePath);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);
            File.Delete(submissionFilePath);
            return submissionFilePath;
        }

        protected virtual string PreprocessJsSubmission(string template, ExecutionContext context, string pathToFile)
        {
            var processedCode =
                template.Replace(RequiredModules, this.JsCodeRequiredModules)
                    .Replace(PreevaluationPlaceholder, this.JsCodePreevaulationCode)
                    .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                    .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                    .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
                    .Replace(UserInputPlaceholder, pathToFile)
                    .Replace(TestsPlaceholder, this.BuildTests(context.Tests));

            return processedCode;
        }
    }
}
