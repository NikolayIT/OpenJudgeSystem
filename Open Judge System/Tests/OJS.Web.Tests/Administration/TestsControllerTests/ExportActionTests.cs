namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using System.Linq;
    using System.Web.Mvc;
    
    using Ionic.Zip;

    using NUnit.Framework;

    using OJS.Web.Common.ZippedTestManipulator;

    [TestFixture]
    public class ExportActionTests : TestsControllerBaseTestsClass
    {
        [Test]
        public void ExportActionShouldReturnNotNullableResult()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "Problem").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.IsNotNull(result);
        }

        [Test]
        public void ExportActionShouldReturnZipFile()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "Problem").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.IsTrue(zipFile is ZipFile);
        }

        [Test]
        public void ExportActionShouldReturnZipFileWith24Files()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "Problem").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.AreEqual(zipFile.Count, 26);
        }

        [Test]
        public void ExportActionShouldReturnCorrectTrialTestFiles()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "Problem").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.IsTrue(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.in.txt", 1)));
            Assert.IsTrue(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.out.txt", 1)));
            Assert.IsTrue(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.in.txt", 2)));
            Assert.IsTrue(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.out.txt", 2)));
            Assert.IsFalse(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.in.txt", 3)));
            Assert.IsFalse(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.out.txt", 3)));
        }

        [Test]
        public void ExportActionShouldNotHaveNotNeededTrialTestFiles()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "Problem").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.IsFalse(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.in.txt", 3)));
            Assert.IsFalse(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.out.txt", 3)));
        }

        [Test]
        public void ExportActionShouldReturnCorrectNonTrialTestFiles()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "Problem").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            for (int i = 1; i <= 10; i++)
            {
                Assert.IsTrue(zipFile.EntryFileNames.Contains(string.Format("test.{0:D3}.in.txt", i)));
                Assert.IsTrue(zipFile.EntryFileNames.Contains(string.Format("test.{0:D3}.out.txt", i)));
            }
        }

        [Test]
        public void ExportActionShouldNotHaveNotNeededNonTrialTestFiles()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "Problem").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.IsFalse(zipFile.EntryFileNames.Contains(string.Format("test.{0:D3}.in.txt", 12)));
            Assert.IsFalse(zipFile.EntryFileNames.Contains(string.Format("test.{0:D3}.out.txt", 12)));
        }

        [Test]
        public void ExportActionShouldReturnCorrectTrialTestFileContent()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "Problem").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.AreEqual(ZippedTestsManipulator.ExtractFileFromStream(zipFile.EntriesSorted.Where(x => x.FileName == "test.000.001.in.txt").FirstOrDefault()), "Trial input test 1");
            Assert.AreEqual(ZippedTestsManipulator.ExtractFileFromStream(zipFile.EntriesSorted.Where(x => x.FileName == "test.000.001.out.txt").FirstOrDefault()), "Trial output test 1");
            Assert.AreEqual(ZippedTestsManipulator.ExtractFileFromStream(zipFile.EntriesSorted.Where(x => x.FileName == "test.000.002.in.txt").FirstOrDefault()), "Trial input test 2");
            Assert.AreEqual(ZippedTestsManipulator.ExtractFileFromStream(zipFile.EntriesSorted.Where(x => x.FileName == "test.000.002.out.txt").FirstOrDefault()), "Trial output test 2");
        }

        [Test]
        public void ExportActionShouldReturnCorrectNonTrialTestFileContent()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "Problem").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(ZippedTestsManipulator.ExtractFileFromStream(zipFile.EntriesSorted.Where(x => x.FileName == string.Format("test.{0:D3}.in.txt", i + 1)).FirstOrDefault()), i.ToString());
                Assert.AreEqual(ZippedTestsManipulator.ExtractFileFromStream(zipFile.EntriesSorted.Where(x => x.FileName == string.Format("test.{0:D3}.out.txt", i + 1)).FirstOrDefault()), (i + 1).ToString());
            }
        }

        [Test]
        public void ExportActionShouldReturnOnlyTrialTestsIfTheProblemDoesNotHaveNonTrial()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "OnlyTrialTests").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.AreEqual(2, zipFile.Count);
        }

        [Test]
        public void ExportActionShouldReturnProperTrialTestsIfTheProblemDoesNotHaveNonTrial()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "OnlyTrialTests").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.IsTrue(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.in.txt", 1)));
            Assert.IsTrue(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.out.txt", 1)));
        }

        [Test]
        public void ExportActionShoudlNotHaveNotNeededTrialTestsIfTheProblemDoesNotHaveNonTrial()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "OnlyTrialTests").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.IsFalse(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.in.txt", 2)));
            Assert.IsFalse(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.out.txt", 2)));
        }

        [Test]
        public void ExportActionShouldReturnProperTrialTestsContentIfTheProblemDoesNotHaveNonTrialTests()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "OnlyTrialTests").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.AreEqual(ZippedTestsManipulator.ExtractFileFromStream(zipFile.EntriesSorted.Where(x => x.FileName == "test.000.001.in.txt").FirstOrDefault()), "Zero test 1\nZero test 1 second line");
            Assert.AreEqual(ZippedTestsManipulator.ExtractFileFromStream(zipFile.EntriesSorted.Where(x => x.FileName == "test.000.001.out.txt").FirstOrDefault()), "Zero test 1\nZero test 1 second lint output");
        }

        [Test]
        public void ExportActionShouldReturnOnlyNonTrialTestsIfProblemDoesNotHaveTrialOnes()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "OnlyNormalTests").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.AreEqual(20, zipFile.Count);
        }

        [Test]
        public void ExportActionShouldReturnOnlyNonTrialTestsNamesIfProblemDoesNotHaveTrialOnes()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "OnlyNormalTests").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            for (int i = 1; i <= 10; i++)
            {
                Assert.IsTrue(zipFile.EntryFileNames.Contains(string.Format("test.{0:D3}.in.txt", i)));
                Assert.IsTrue(zipFile.EntryFileNames.Contains(string.Format("test.{0:D3}.out.txt", i)));
            }
        }

        [Test]
        public void ExportActionShouldNotReturnNotNeededFilesIfNoOnlyNonTrialTestsNamesIfProblemDoesNotHaveTrialOnes()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "OnlyNormalTests").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            Assert.IsFalse(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.in.txt", 1)));
            Assert.IsFalse(zipFile.EntryFileNames.Contains(string.Format("test.000.{0:D3}.out.txt", 1)));
        }

        [Test]
        public void ExportActionShouldReturnOnlyNonTrialTestsContentIfProblemDoesNotHaveTrialOnes()
        {
            var problemId = this.Data.Problems.All().Where(x => x.Name == "OnlyNormalTests").FirstOrDefault().Id;

            var result = TestsController
                .Export(problemId) as FileStreamResult;

            var zipFile = ZipFile.Read(result.FileStream);

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(ZippedTestsManipulator.ExtractFileFromStream(zipFile.EntriesSorted.Where(x => x.FileName == string.Format("test.{0:D3}.in.txt", i + 1)).FirstOrDefault()), "Only normal tests " + i.ToString());
                Assert.AreEqual(ZippedTestsManipulator.ExtractFileFromStream(zipFile.EntriesSorted.Where(x => x.FileName == string.Format("test.{0:D3}.out.txt", i + 1)).FirstOrDefault()), "Only normal tests output" + i.ToString());
            }
        }
    }
}
