namespace OJS.Workers.Tools.Tests.AntiCheat
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using OJS.Workers.Tools.AntiCheat;

    [TestFixture]
    public class SortAndTrimLinesVisitorTests
    {
        [Test]
        public void TestWith9LinesInRevertedOrder()
        {
            var data = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var input = string.Join(Environment.NewLine, data.Reverse());
            var expectedOutput = string.Join(Environment.NewLine, data) + Environment.NewLine;
            var visitor = new SortAndTrimLinesVisitor();
            var result = visitor.Visit(input);
            Assert.AreEqual(expectedOutput, result);
        }
    }
}
