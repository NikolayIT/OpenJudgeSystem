namespace OJS.Workers.Executors.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Forms;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Workers.Common;

    [TestClass]
    public class TestRestrictedProcess : BaseExecutorsTestClass
    {
        [TestMethod]
        public void RestrictedProcessShouldStopProgramAfterTimeIsEnded()
        {
            var exePath = this.CreateExe("TimeLimit.exe", TimeLimitSourceCode);
            
            var process = new RestrictedProcessExecutor();
            var result = process.Execute(exePath, string.Empty, 100, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.TimeLimit);
        }

        [TestMethod]
        public void RestrictedProcessShouldSendInputDataToProcess()
        {
            var exePath = this.CreateExe("InputOutput.exe", ReadInputAndThenOutputSourceCode);
            
            const string InputData = "SomeInputData!!@#$%^&*(\n";
            var process = new RestrictedProcessExecutor();
            var result = process.Execute(exePath, InputData, 2000, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.AreEqual(InputData.Trim(), result.ReceivedOutput.Trim());
        }

        [TestMethod]
        public void RestrictedProcessShouldNotBeAbleToReadClipboard()
        {
            Clipboard.SetText("clipboard test");
            var exePath = this.CreateExe("ReadClipboard.exe", ReadClipboardSourceCode);
            
            var process = new RestrictedProcessExecutor();
            var result = process.Execute(exePath, string.Empty, 1500, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.RunTimeError, "No exception is thrown!");
        }

        [TestMethod]
        public void RestrictedProcessShouldNotBeAbleToWriteToClipboard()
        {
            var exePath = this.CreateExe("WriteToClipboard.exe", WriteToClipboardSourceCode);

            var process = new RestrictedProcessExecutor();
            var result = process.Execute(exePath, string.Empty, 1500, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.RunTimeError, "No exception is thrown!");
            Assert.AreNotEqual("i did it", Clipboard.GetText());
        }

        [TestMethod]
        public void RestrictedProcessShouldNotBeAbleToStartProcess()
        {
            var notepadsBefore = Process.GetProcessesByName("notepad.exe").Count();
            var exePath = this.CreateExe("StartProcess.exe", StartNotepadProcessSourceCode);

            var process = new RestrictedProcessExecutor();
            var result = process.Execute(exePath, string.Empty, 1500, 32 * 1024 * 1024);

            var notepadsAfter = Process.GetProcessesByName("notepad.exe").Count();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.RunTimeError, "No exception is thrown!");
            Assert.AreEqual(notepadsBefore, notepadsAfter);
        }

        [TestMethod]
        public void RestrictedProcessShouldNotBlockWhenEnterEndlessLoop()
        {
            var exePath = this.CreateExe("EndlessLoop.exe", EndlessLoopSourceCode);

            var process = new RestrictedProcessExecutor();
            var result = process.Execute(exePath, string.Empty, 50, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.TimeLimit);
        }

        [TestMethod]
        public void RestrictedProcessShouldStandardErrorContentShouldContainExceptions()
        {
            var exePath = this.CreateExe("ThrowException.exe", ThrowExceptionSourceCode);

            var process = new RestrictedProcessExecutor();
            var result = process.Execute(exePath, string.Empty, 500, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.RunTimeError, "No exception is thrown!");
            Assert.IsTrue(result.ErrorOutput.Contains("Exception message!"));
        }

        [TestMethod]
        public void RestrictedProcessShouldReturnCorrectAmountOfUsedMemory()
        {

            var exePath = this.CreateExe("Consuming50MbOfMemory.exe", Consuming50MbOfMemorySourceCode);

            var process = new RestrictedProcessExecutor();
            var result = process.Execute(exePath, string.Empty, 5000, 100 * 1024 * 1024);

            Console.WriteLine(result.MemoryUsed);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.MemoryUsed > 50 * 1024 * 1024);
        }

        [TestMethod]
        public void RestrictedProcessShouldReturnMemoryLimitWhenNeeded()
        {
            var exePath = this.CreateExe("Consuming50MbOfMemory.exe", Consuming50MbOfMemorySourceCode);

            var process = new RestrictedProcessExecutor();
            var result = process.Execute(exePath, string.Empty, 5000, 30 * 1024 * 1024);

            Console.WriteLine(result.MemoryUsed);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.MemoryLimit);
        }
    }
}
