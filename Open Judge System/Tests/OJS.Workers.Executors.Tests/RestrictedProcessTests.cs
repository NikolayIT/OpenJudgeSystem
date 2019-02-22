namespace OJS.Workers.Executors.Tests
{
    using System;

    using NUnit.Framework;

    using OJS.Workers.Common;

    [TestFixture]
    public class RestrictedProcessTests : BaseExecutorsTestClass
    {
        private const string ReadInputAndThenOutputSourceCode = @"using System;
class Program
{
    public static void Main()
    {
        var line = Console.ReadLine();
        Console.WriteLine(line);
    }
}";

        private const string Consuming50MbOfMemorySourceCode = @"using System;
using System.Windows.Forms;
class Program
{
    public static void Main()
    {
        var array = new int[50 * 1024 * 1024 / 4];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = i;
        }
        Console.WriteLine(array[12345]);
    }
}";

        [Test]
        public void RestrictedProcessShouldStopProgramAfterTimeIsEnded()
        {
            const string TimeLimitSourceCode = @"using System;
using System.Threading;
class Program
{
    public static void Main()
    {
        Thread.Sleep(150);
    }
}";
            var exePath = this.CreateExe("RestrictedProcessShouldStopProgramAfterTimeIsEnded.exe", TimeLimitSourceCode);

            var result = this.RestrictedProcess.Execute(
                exePath,
                string.Empty,
                DefaultTimeLimit,
                DefaultMemoryLimit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.TimeLimit);
        }

        [Test]
        public void RestrictedProcessShouldSendInputDataToProcess()
        {
            var exePath = this.CreateExe("RestrictedProcessShouldSendInputDataToProcess.exe", ReadInputAndThenOutputSourceCode);

            const string InputData = "SomeInputData!!@#$%^&*(\n";

            var result = this.RestrictedProcess.Execute(
                exePath,
                InputData,
                DefaultTimeLimit * 20,
                DefaultMemoryLimit);

            Assert.IsNotNull(result);
            Assert.AreEqual(InputData.Trim(), result.ReceivedOutput.Trim());
        }

        [Test]
        public void RestrictedProcessShouldWorkWithCyrillic()
        {
            var exePath = this.CreateExe("RestrictedProcessShouldWorkWithCyrillic.exe", ReadInputAndThenOutputSourceCode);

            const string InputData = "Николай\n";

            var result = this.RestrictedProcess.Execute(
                exePath,
                InputData,
                DefaultTimeLimit * 20,
                DefaultMemoryLimit);

            Assert.IsNotNull(result);
            Assert.AreEqual(InputData.Trim(), result.ReceivedOutput.Trim());
        }

        [Test]
        public void RestrictedProcessShouldOutputProperLengthForCyrillicText()
        {
            const string ReadInputAndThenOutputTheLengthSourceCode = @"using System;
class Program
{
    public static void Main()
    {
        var line = Console.ReadLine();
        Console.WriteLine(line.Length);
    }
}";
            var exePath = this.CreateExe("RestrictedProcessShouldOutputProperLengthForCyrillicText.exe", ReadInputAndThenOutputTheLengthSourceCode);

            const string InputData = "Николай\n";

            var result = this.RestrictedProcess.Execute(
                exePath,
                InputData,
                DefaultTimeLimit * 20,
                DefaultMemoryLimit,
                null,
                null,
                false,
                useSystemEncoding: true);

            Assert.IsNotNull(result);
            Assert.AreEqual("7", result.ReceivedOutput.Trim());
        }

        [Test]
        public void RestrictedProcessShouldReceiveCyrillicText()
        {
            const string ReadInputAndThenCheckTheTextToContainCyrillicLettersSourceCode = @"using System;
class Program
{
    public static void Main()
    {
        var line = Console.ReadLine();
        Console.WriteLine((line.Contains(""а"") || line.Contains(""е"")));
    }
}";
            var exePath = this.CreateExe("RestrictedProcessShouldReceiveCyrillicText.exe", ReadInputAndThenCheckTheTextToContainCyrillicLettersSourceCode);

            const string InputData = "абвгдежзийклмнопрстуфхцчшщъьюя\n";

            var result = this.RestrictedProcess.Execute(
                exePath,
                InputData,
                DefaultTimeLimit * 20,
                DefaultMemoryLimit,
                null,
                null,
                false,
                useSystemEncoding: true);

            Assert.IsNotNull(result);
            Assert.AreEqual("True", result.ReceivedOutput.Trim());
        }

        [Test]
        public void RestrictedProcessShouldNotBlockWhenEnterEndlessLoop()
        {
            const string EndlessLoopSourceCode = @"using System;
class Program
{
    public static void Main()
    {
        while(true) { }
    }
}";
            var exePath = this.CreateExe("RestrictedProcessShouldNotBlockWhenEnterEndlessLoop.exe", EndlessLoopSourceCode);

            var result = this.RestrictedProcess.Execute(
                exePath,
                string.Empty,
                DefaultTimeLimit / 2,
                DefaultMemoryLimit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.TimeLimit);
        }

        [Test]
        public void RestrictedProcessStandardErrorContentShouldContainExceptions()
        {
            const string ThrowExceptionSourceCode = @"using System;
using System.Windows.Forms;
class Program
{
    public static void Main()
    {
        throw new Exception(""Exception message!"");
    }
}";
            var exePath = this.CreateExe("RestrictedProcessShouldStandardErrorContentShouldContainExceptions.exe", ThrowExceptionSourceCode);

            var result = this.RestrictedProcess.Execute(
                exePath,
                string.Empty,
                DefaultTimeLimit * 5,
                DefaultMemoryLimit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.RunTimeError, "No exception is thrown!");
            Assert.IsTrue(result.ErrorOutput.Contains("Exception message!"));
        }

        [Test]
        public void RestrictedProcessShouldReturnCorrectAmountOfUsedMemory()
        {
            var exePath = this.CreateExe("RestrictedProcessShouldReturnCorrectAmountOfUsedMemory.exe", Consuming50MbOfMemorySourceCode);

            var result = this.RestrictedProcess.Execute(
                exePath,
                string.Empty,
                DefaultTimeLimit * 50,
                DefaultMemoryLimit);

            Console.WriteLine(result.MemoryUsed);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.MemoryUsed > 50 * 1024 * 1024);
        }

        [Test]
        public void RestrictedProcessShouldReturnMemoryLimitWhenNeeded()
        {
            var exePath = this.CreateExe("RestrictedProcessShouldReturnMemoryLimitWhenNeeded.exe", Consuming50MbOfMemorySourceCode);

            var result = this.RestrictedProcess.Execute(
                exePath,
                string.Empty,
                DefaultTimeLimit * 50,
                DefaultMemoryLimit);

            Console.WriteLine(result.MemoryUsed);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == ProcessExecutionResultType.MemoryLimit);
        }
    }
}
