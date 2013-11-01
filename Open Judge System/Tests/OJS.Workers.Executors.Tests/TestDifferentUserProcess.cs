namespace OJS.Workers.Executors.Tests
{
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Forms;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestDifferentUserProcess : BaseExecutorsTestClass
    {
        [TestMethod]
        public void DifferentUserProcessShouldStopProgramAfterTimeIsEnded()
        {
            var exePath = this.CreateExe("TimeLimit.exe", TimeLimitSourceCode);

            var process = new DifferentUserProcessExecutor(exePath, string.Empty, string.Empty, string.Empty);
            var result = process.Start(100, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ProcessKilledBecauseOfTimeLimit);
        }

        [TestMethod]
        public void DifferentUserProcessShouldSendInputDataToProcess()
        {
            var exePath = this.CreateExe("InputOutput.exe", ReadInputAndThenOutputSourceCode);

            const string InputData = "SomeInputData!!@#$%^&*(\n";
            var process = new DifferentUserProcessExecutor(exePath, string.Empty, string.Empty, string.Empty);
            process.SetTextToWrite(InputData);
            var result = process.Start(1000, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.AreEqual(InputData.Trim(), result.StandardOutputContent.Trim());
        }

        [TestMethod]
        public void DifferentUserProcessShouldNotBeAbleToReadClipboard()
        {
            Clipboard.SetText("clipboard test");
            var exePath = this.CreateExe("ReadClipboard.exe", ReadClipboardSourceCode);

            var process = new DifferentUserProcessExecutor(exePath, string.Empty, string.Empty, string.Empty);
            var result = process.Start(1500, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result.StandardErrorContent), "No exception is thrown!"); // Exception is thrown
        }

        [TestMethod]
        public void DifferentUserProcessShouldNotBeAbleToWriteToClipboard()
        {
            var exePath = this.CreateExe("WriteToClipboard.exe", WriteToClipboardSourceCode);

            var process = new DifferentUserProcessExecutor(exePath, string.Empty, string.Empty, string.Empty);
            var result = process.Start(1500, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result.StandardErrorContent), "No exception is thrown!"); // Exception is thrown
            Assert.AreNotEqual("i did it", Clipboard.GetText());
        }

        [TestMethod]
        public void DifferentUserProcessShouldNotBeAbleToStartProcess()
        {
            var notepadsBefore = Process.GetProcessesByName("notepad.exe").Count();
            var exePath = this.CreateExe("StartProcess.exe", StartNotepadProcessSourceCode);

            var process = new DifferentUserProcessExecutor(exePath, string.Empty, string.Empty, string.Empty);
            var result = process.Start(1500, 32 * 1024 * 1024);
            var notepadsAfter = Process.GetProcessesByName("notepad.exe").Count();

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result.StandardErrorContent), "No exception is thrown!"); // Exception is thrown
            Assert.AreEqual(notepadsBefore, notepadsAfter);
        }

        [TestMethod]
        public void DifferentUserProcessShouldNotBlockWhenEnterEndlessLoop()
        {
            var exePath = this.CreateExe("EndlessLoop.exe", EndlessLoopSourceCode);

            var process = new DifferentUserProcessExecutor(exePath, string.Empty, string.Empty, string.Empty);
            var result = process.Start(50, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ProcessKilledBecauseOfTimeLimit);
        }

        [TestMethod]
        public void DifferentUserProcessShouldStandardErrorContentShouldContainExceptions()
        {
            var exePath = this.CreateExe("ThrowException.exe", ThrowExceptionSourceCode);

            var process = new DifferentUserProcessExecutor(exePath, string.Empty, string.Empty, string.Empty);
            var result = process.Start(500, 32 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result.StandardErrorContent), "StandardErrorContent is empty!"); // Exception is thrown
            Assert.IsTrue(result.StandardErrorContent.Contains("Exception message!"));
        }

        [TestMethod]
        public void DifferentUserProcessShouldReturnCorrectAmountOfUsedMemory()
        {
            var exePath = this.CreateExe("Consuming50MbOfMemory.exe", Consuming50MbOfMemorySourceCode);

            var process = new DifferentUserProcessExecutor(exePath, string.Empty, string.Empty, string.Empty);
            var result = process.Start(5000, 100 * 1024 * 1024);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.MaxMemoryUsed > 50 * 1024 * 1024);
        }
    }
}
