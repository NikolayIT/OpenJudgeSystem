namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class JavaPreprocessCompileExecuteAndCheckExecutionStrategy : ExecutionStrategy
    {
        private const string PackageNameRegEx = @"\bpackage\s+[a-zA-Z_][a-zA-Z_.0-9]{0,150}\s*;";
        private const string ClassNameRegEx = @"public\s+class\s+([a-zA-Z_][a-zA-Z_0-9]{0,50})\s*{";
        private const string TimeMeasurementFileName = "_time.txt";
        private const string SandboxExecutorClassName = "_$SandboxExecutor";

        private readonly string javaExecutablePath;
        private readonly string workingDirectory;
        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public JavaPreprocessCompileExecuteAndCheckExecutionStrategy(string javaExecutablePath, Func<CompilerType, string> getCompilerPathFunc)
        {
            if (!File.Exists(javaExecutablePath))
            {
                throw new ArgumentException(string.Format("Java not found in: {0}!", javaExecutablePath), "javaExecutablePath");
            }

            this.javaExecutablePath = javaExecutablePath;
            this.workingDirectory = DirectoryHelpers.CreateTempDirectory();
            this.getCompilerPathFunc = getCompilerPathFunc;
        }

        ~JavaPreprocessCompileExecuteAndCheckExecutionStrategy()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.workingDirectory, true);
        }

        private string SandboxExecutorCode => @"
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

            // Create a temp file with the submission code
            string submissionFilePath;
            try
            {
                submissionFilePath = this.CreateSubmissionFile(executionContext.Code);
            }
            catch (ArgumentException exception)
            {
                result.IsCompiledSuccessfully = false;
                result.CompilerComment = exception.Message;

                return result;
            }

            // Create a sandbox executor source file in the working directory
            var sandboxExecutorSourceFilePath = $"{this.workingDirectory}\\{SandboxExecutorClassName}";
            File.WriteAllText(sandboxExecutorSourceFilePath, this.SandboxExecutorCode);

            // Compile all source files - sandbox executor and submission file
            var compilerPath = this.getCompilerPathFunc(executionContext.CompilerType);
            var compilerResult = this.CompileSourceFiles(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                new[] { submissionFilePath, sandboxExecutorSourceFilePath });

            // Assign compiled result info to the execution result
            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;
            if (!result.IsCompiledSuccessfully)
            {
                return result;
            }

            // Prepare execution process arguments and time measurement info
            var classPathArgument = $"-classpath \"{this.workingDirectory}\"";

            var submissionFilePathLastIndexOfSlash = submissionFilePath.LastIndexOf('\\');
            var classToExecute = submissionFilePath.Substring(submissionFilePathLastIndexOfSlash + 1);

            var timeMeasurementFilePath = $"{this.workingDirectory}\\{TimeMeasurementFileName}";

            // Create an executor and a checker
            var executor = new StandardProcessExecutor();
            var checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);

            // Process the submission and check each test
            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(
                    this.javaExecutablePath,
                    test.Input,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    new[] { classPathArgument, SandboxExecutorClassName, classToExecute, $"\"{timeMeasurementFilePath}\"" });

                UpdateExecutionTime(timeMeasurementFilePath, processExecutionResult, executionContext.TimeLimit);

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        private static void UpdateExecutionTime(string timeMeasurementFilePath, ProcessExecutionResult processExecutionResult, int timeLimit)
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
                }

                File.Delete(timeMeasurementFilePath);
            }
        }

        private string CreateSubmissionFile(string submissionCode)
        {
            // Remove existing packages
            submissionCode = Regex.Replace(submissionCode, PackageNameRegEx, string.Empty);

            // TODO: Remove the restriction for one public class - a non-public Java class can contain the main method!
            var classNameMatch = Regex.Match(submissionCode, ClassNameRegEx);
            if (!classNameMatch.Success)
            {
                throw new ArgumentException("No valid public class found!");
            }

            var className = classNameMatch.Groups[1].Value;
            var submissionFilePath = $"{this.workingDirectory}\\{className}";

            File.WriteAllText(submissionFilePath, submissionCode);

            return submissionFilePath;
        }

        private CompileResult CompileSourceFiles(CompilerType compilerType, string compilerPath, string compilerArguments, IEnumerable<string> sourceFilesToCompile)
        {
            CompileResult compilerResult = null;
            foreach (var sourceFile in sourceFilesToCompile)
            {
                compilerResult = this.Compile(compilerType, compilerPath, compilerArguments, sourceFile);

                if (!compilerResult.IsCompiledSuccessfully)
                {
                    break; // The compilation of other files is not necessary
                }
            }

            return compilerResult;
        }
    }
}
