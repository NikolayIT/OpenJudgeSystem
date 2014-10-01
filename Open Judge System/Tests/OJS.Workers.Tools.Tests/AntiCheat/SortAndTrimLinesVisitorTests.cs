namespace OJS.Workers.Tools.Tests.AntiCheat
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Workers.Tools.AntiCheat;

    [TestClass]
    public class SortAndTrimLinesVisitorTests
    {
        [TestMethod]
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
