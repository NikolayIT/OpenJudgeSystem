namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Executors;

    public class JavaPreprocessCompileExecuteAndCheckExecutionStrategy : ExecutionStrategy
    {
        protected const string TimeMeasurementFileName = "_$time.txt";
        protected const string SandboxExecutorClassName = "_$SandboxExecutor";
        protected const string JavaCompiledFileExtension = ".class";

        public JavaPreprocessCompileExecuteAndCheckExecutionStrategy(
            string javaExecutablePath,
            Func<CompilerType, string> getCompilerPathFunc,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed)
        {
            if (!File.Exists(javaExecutablePath))
            {
                throw new ArgumentException($"Java not found in: {javaExecutablePath}!", nameof(javaExecutablePath));
            }

            this.JavaExecutablePath = javaExecutablePath;
            this.GetCompilerPathFunc = getCompilerPathFunc;
        }

        protected string JavaExecutablePath { get; }

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        protected string SandboxExecutorSourceFilePath =>
            $"{this.WorkingDirectory}\\{SandboxExecutorClassName}{GlobalConstants.JavaSourceFileExtension}";

        protected string SandboxExecutorCode => @"
import java.io.File;
import java.io.FilePermission;
import java.io.FileWriter;
import java.lang.reflect.Method;
import java.lang.reflect.ReflectPermission;
import java.net.NetPermission;
import java.security.Permission;
import java.util.PropertyPermission;

public class " + SandboxExecutorClassName + @" {
    private static final String MAIN_METHOD_NAME = ""main"";

    public static void main(String[] args) throws Throwable {
        if (args.length == 0) {
            throw new IllegalArgumentException(""The name of the class to execute not provided!"");
        }

        String className = args[0];
        Class<?> userClass = Class.forName(className);

        Method mainMethod = userClass.getMethod(MAIN_METHOD_NAME, String[].class);

        FileWriter writer = null;
        long startTime = 0;
        try {
            if (args.length == 2) {
                String timeFilePath = args[1];
                writer = new FileWriter(timeFilePath, false);
            }

            // Set the sandbox security manager
            _$SandboxSecurityManager securityManager = new _$SandboxSecurityManager();
            System.setSecurityManager(securityManager);

            startTime = System.nanoTime();

            mainMethod.invoke(userClass, (Object) args);
        } catch (Throwable throwable) {
            Throwable cause = throwable.getCause();
            throw cause == null ? throwable : cause;
        } finally {
            if (writer != null) {
                long endTime = System.nanoTime();
                writer.write("""" + (endTime - startTime));
                writer.close();
            }
        }
    }
}

class _$SandboxSecurityManager extends SecurityManager {
    private static final String JAVA_HOME_DIR = System.getProperty(""java.home"");
    private static final String USER_DIR = System.getProperty(""user.dir"");
    private static final String EXECUTING_FILE_PATH = _$SandboxSecurityManager.class.getProtectionDomain().getCodeSource().getLocation().getPath();

    @Override
    public void checkPermission(Permission permission) {
        if (permission instanceof PropertyPermission) {
            // Allow reading system properties
            return;
        }

        if (permission instanceof FilePermission) {
            FilePermission filePermission = (FilePermission) permission;
            String fileName = filePermission.getName();
            String filePath = new File(fileName).getPath();

            if (filePermission.getActions().equals(""read"") &&
                    (filePath.startsWith(JAVA_HOME_DIR) ||
                        filePath.startsWith(USER_DIR) ||
                        filePath.startsWith(new File(EXECUTING_FILE_PATH).getPath()))) {
                    // Allow reading Java system directories and user directories
                    return;
                }
            }

        if (permission instanceof NetPermission) {
            if (permission.getName().equals(""specifyStreamHandler"")) {
                // Allow specifyStreamHandler
                return;
            }
        }

        if (permission instanceof ReflectPermission) {
            if (permission.getName().equals(""suppressAccessChecks"")) {
                // Allow suppressAccessChecks
                return;
            }
        }

        if (permission instanceof RuntimePermission) {
            if (permission.getName().equals(""createClassLoader"") ||
                    permission.getName().startsWith(""accessClassInPackage.sun."") ||
                    permission.getName().equals(""getProtectionDomain"") ||
                    permission.getName().equals(""accessDeclaredMembers"")) {
                // Allow createClassLoader, accessClassInPackage.sun, getProtectionDomain and accessDeclaredMembers
                return;
            }
        }

        throw new SecurityException(""Not allowed: "" + permission.getClass().getName());
    }

    @Override
    public void checkAccess(Thread thread) {
        throw new UnsupportedOperationException();
    }
}";

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            // Copy the sandbox executor source code to a file in the working directory
            File.WriteAllText(this.SandboxExecutorSourceFilePath, this.SandboxExecutorCode);

            // Create a temp file with the submission code
            string submissionFilePath;
            try
            {
                submissionFilePath = this.CreateSubmissionFile(executionContext);
            }
            catch (ArgumentException exception)
            {
                result.IsCompiledSuccessfully = false;
                result.CompilerComment = exception.Message;

                return result;
            }

            var compilerResult = this.DoCompile(executionContext, submissionFilePath);

            // Assign compiled result info to the execution result
            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;
            if (!result.IsCompiledSuccessfully)
            {
                return result;
            }

            // Prepare execution process arguments and time measurement info
            var classPathArgument = $"-classpath \"{this.WorkingDirectory}\"";

            var classToExecuteFilePath = compilerResult.OutputFile;
            var classToExecute = classToExecuteFilePath
                .Substring(
                    this.WorkingDirectory.Length + 1,
                    classToExecuteFilePath.Length - this.WorkingDirectory.Length - JavaCompiledFileExtension.Length - 1)
                .Replace('\\', '.');

            var timeMeasurementFilePath = $"{this.WorkingDirectory}\\{TimeMeasurementFileName}";

            // Create an executor and a checker
            var executor = new StandardProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);
            var checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);

            // Process the submission and check each test
            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(
                    this.JavaExecutablePath,
                    test.Input,
                    executionContext.TimeLimit * 2, // Java virtual machine takes more time to start up
                    executionContext.MemoryLimit,
                    new[] { classPathArgument, SandboxExecutorClassName, classToExecute, $"\"{timeMeasurementFilePath}\"" },
                    null,
                    false,
                    true);

                UpdateExecutionTime(timeMeasurementFilePath, processExecutionResult, executionContext.TimeLimit);

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        protected static void UpdateExecutionTime(string timeMeasurementFilePath, ProcessExecutionResult processExecutionResult, int timeLimit)
        {
            if (File.Exists(timeMeasurementFilePath))
            {
                long timeInNanoseconds;
                var timeMeasurementFileContent = File.ReadAllText(timeMeasurementFilePath);
                if (long.TryParse(timeMeasurementFileContent, out timeInNanoseconds))
                {
                    processExecutionResult.TimeWorked = TimeSpan.FromMilliseconds((double)timeInNanoseconds / 1000000);

                    if (processExecutionResult.Type == ProcessExecutionResultType.TimeLimit &&
                        processExecutionResult.TimeWorked.TotalMilliseconds <= timeLimit)
                    {
                        // The time from the time measurement file is under the time limit
                        processExecutionResult.Type = ProcessExecutionResultType.Success;
                    }
                    else if (processExecutionResult.Type == ProcessExecutionResultType.Success &&
                        processExecutionResult.TimeWorked.TotalMilliseconds > timeLimit)
                    {
                        processExecutionResult.Type = ProcessExecutionResultType.TimeLimit;
                    }
                }

                File.Delete(timeMeasurementFilePath);
            }
        }

        protected virtual string CreateSubmissionFile(ExecutionContext executionContext) =>
            JavaCodePreprocessorHelper.CreateSubmissionFile(executionContext.Code, this.WorkingDirectory);

        protected virtual CompileResult DoCompile(ExecutionContext executionContext, string submissionFilePath)
        {
            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            // Compile all source files - sandbox executor and submission file
            var compilerResult = this.CompileSourceFiles(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                new[] { this.SandboxExecutorSourceFilePath, submissionFilePath });

            return compilerResult;
        }

        private CompileResult CompileSourceFiles(
            CompilerType compilerType, 
            string compilerPath,
            string compilerArguments, 
            IEnumerable<string> sourceFilesToCompile)
        {
            var compilerResult = new CompileResult(false, null);
            var compilerCommentBuilder = new StringBuilder();

            foreach (var sourceFile in sourceFilesToCompile)
            {
                compilerResult = this.Compile(compilerType, compilerPath, compilerArguments, sourceFile);

                compilerCommentBuilder.AppendLine(compilerResult.CompilerComment);

                if (!compilerResult.IsCompiledSuccessfully)
                {
                    break; // The compilation of other files is not necessary
                }
            }

            var compilerComment = compilerCommentBuilder.ToString().Trim();
            compilerResult.CompilerComment = compilerComment.Length > 0 ? compilerComment : null;

            return compilerResult;
        }
    }
}