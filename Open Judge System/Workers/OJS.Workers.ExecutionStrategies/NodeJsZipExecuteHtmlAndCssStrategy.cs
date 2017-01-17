namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using Checkers;
    using Common;
    using Executors;
    using Ionic.Zip;
    using OJS.Common;
    using OJS.Common.Extensions;

    public class NodeJsZipExecuteHtmlAndCssStrategy : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy
    {
        protected const string EntryFileName = "*.html";
        protected const string SubmissionFileName = "_$submission";

        public NodeJsZipExecuteHtmlAndCssStrategy(
            string nodeJsExecutablePath,
            string mochaModulePath,
            string chaiModulePath,
            string jsdomModulePath,
            string jqueryModulePath,
            string underscoreModulePath,
            string bootsrapModulePath,
            string bootstrapCssPath,
            int baseTimeUsed,
            int baseMemoryUsed)
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
                throw new ArgumentException($"jsDom not found in: {jsdomModulePath}");
            }

            if (!Directory.Exists(jqueryModulePath))
            {
                throw new ArgumentException($"jQuery not found in: {jqueryModulePath}");
            }

            if (!File.Exists(bootsrapModulePath))
            {
                throw new ArgumentException($"Bootstrap Module not found in: {bootsrapModulePath}");
            }

            if (!File.Exists(bootstrapCssPath))
            {
                throw new ArgumentException($"Bootstrap CSS not found in: {bootstrapCssPath}");
            }

            this.JsDomModulePath = this.ProcessModulePath(jsdomModulePath);
            this.JQueryModulePath = this.ProcessModulePath(jqueryModulePath);
            this.BootstrapModulePath = this.ProcessModulePath(bootsrapModulePath);
            this.BootstrapCssPath = this.ProcessModulePath(bootstrapCssPath);
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectory();
        }

        ~NodeJsZipExecuteHtmlAndCssStrategy()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
        }

        protected string JsDomModulePath { get; }

        protected string JQueryModulePath { get; }

        protected string BootstrapModulePath { get; }

        protected string BootstrapCssPath { get; }

        protected string WorkingDirectory { get; set; }

        protected string ProgramEntryPath { get; set; }

        protected override string JsNodeDisableCode => base.JsNodeDisableCode + @"
fs = undefined;";

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + $@",
    fs = require('fs'),    
    jsdom = require('{this.JsDomModulePath}'),
    jq = require('{this.JQueryModulePath}'),
    bootstrap = fs.readFileSync('{this.BootstrapModulePath}','utf-8'),
    bootstrapCss = fs.readFileSync('{this.BootstrapCssPath}','utf-8'),
    userCode = fs.readFileSync({UserInputPlaceholder},'utf-8')";

        protected override string JsCodeTemplate =>
            RequiredModules + @";" +
            PreevaluationPlaceholder +
            EvaluationPlaceholder +
            PostevaluationPlaceholder;

        protected override string JsCodePreevaulationCode => $@"
describe('TestDOMScope', function() {{
    let bgCoderConsole = {{}};
    before(function(done) {{
        jsdom.env({{
            html: userCode,
            src:[bootstrap],
            done: function(errors, window) {{
                global.window = window;
                global.document = window.document;
                global.$ = global.jQuery = jq(window);
                Object.getOwnPropertyNames(window)
                    .filter(function (prop) {{
                        return prop.toLowerCase().indexOf('html') >= 0;
                    }}).forEach(function (prop) {{
                        global[prop] = window[prop];
                    }});

                let head = $(document.head);
                let style = document.createElement('style');
                style.type = 'text/css';
                style.innerHTML = bootstrapCss;
                head.append(style);

                let links = head.find('link');
                links.each((index, el)=>{{
                    let style = document.createElement('style');
                    style.type = 'test/css';
                    let path = '{this.ProcessModulePath(this.WorkingDirectory)}/' + el.href;
                    let css = fs.readFileSync(path, 'utf-8');
                    style.innerHTML = css;
                    head.append(style);
                }});

                links.remove();    

                Object.keys(console)
                    .forEach(function (prop) {{
                        bgCoderConsole[prop] = console[prop];
                        console[prop] = new Function('');
                    }});

{NodeDisablePlaceholder}

                done();
            }}
        }});
    }});

    after(function() {{
        Object.keys(bgCoderConsole)
            .forEach(function (prop) {{
                console[prop] = bgCoderConsole[prop];
            }});
    }});";

        protected override string JsCodeEvaluation => TestsPlaceholder;

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult { IsCompiledSuccessfully = true };
            this.CreateSubmissionFile(executionContext);
            this.ProgramEntryPath = this.FindProgramEntryPath();

            var codeToExecute = this.PreprocessJsSubmission(
                this.JsCodeTemplate,
                executionContext,
                this.ProgramEntryPath);

            var codeSavePath = FileHelpers.SaveStringToTempFile(codeToExecute);
            var executor = new RestrictedProcessExecutor();

            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result.TestResults = this.ProcessTests(executionContext, executor, checker, codeSavePath);
            File.Delete(codeSavePath);

            return result;
        }

        protected virtual string BuildTests(IEnumerable<TestContext> tests)
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

                    let testFunc = new Function({this.TestFuncVariables}, content);
                    testFunc.call({{}},{this.TestFuncVariables.Replace("'", string.Empty)});

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

        // TODO Extract methods to the Helper class
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
                    EntryFileName,
                    SearchOption.AllDirectories));
            if (files.Count == 0)
            {
                throw new ArgumentException(
                    $@"'{EntryFileName}' file not found in output directory!",
                    nameof(this.WorkingDirectory));
            }

            return this.ProcessModulePath("\"" + files[0] + "\"");
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
