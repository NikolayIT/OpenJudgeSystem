namespace OJS.Web.Tests.Administration.TestsControllerTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using OJS.Data.Models;
    using OJS.Common.Extensions;
    using System.IO;
    using Ionic.Zip;

    [TestClass]
    public class ImportActionTests : TestsControllerBaseTestsClass
    {
        protected Mock<HttpPostedFileBase> file;

        public ImportActionTests()
        {
            this.file = new Mock<HttpPostedFileBase>();
        }

        [TestMethod]
        public void ImportActionShouldShowProperRedirectAndMessageWithIncorrectProblemId()
        {
            var redirectResult = this.testsController.Import("invalid", null, false) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void ImportActionShouldShowProperRedirectAndMessageWhenProblemDoesNotExist()
        {
            var redirectResult = this.testsController.Import("100", null, false) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Невалидна задача", tempDataMessage);
        }

        [TestMethod]
        public void ImportActionShouldShowProperRedirectAndMessageWhenFileIsNull()
        {
            var redirectResult = this.testsController.Import("1", null, false) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);
            Assert.AreEqual(1, redirectResult.RouteValues["id"]);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Файлът не може да бъде празен", tempDataMessage);
        }

        [TestMethod]
        public void ImportActionShouldShowProperRedirectAndMessageWhenFileContentIsZero()
        {
            this.file.Setup(x => x.ContentLength).Returns(0);

            var redirectResult = this.testsController.Import("1", this.file.Object, false) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);
            Assert.AreEqual(1, redirectResult.RouteValues["id"]);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Файлът не може да бъде празен", tempDataMessage);
        }

        [TestMethod]
        public void ImportActionShouldShowProperRedirectAndMessageWhenFileIsNotZip()
        {
            this.file.Setup(x => x.ContentLength).Returns(1);
            this.file.Setup(x => x.FileName).Returns("filename.invalid");

            var redirectResult = this.testsController.Import("1", this.file.Object, false) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);
            Assert.AreEqual(1, redirectResult.RouteValues["id"]);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Файлът трябва да бъде .ZIP файл", tempDataMessage);
        }

        [TestMethod]
        public void ImportActionShouldDeleteAllPreviousTestsIfSettingsAreSetToTrue()
        {
            this.file.Setup(x => x.ContentLength).Returns(1);
            this.file.Setup(x => x.FileName).Returns("filename.zip");

            var remainingTests = this.Data.Tests.All().Where(test => test.ProblemId == 1).Count();

            Assert.AreEqual(13, remainingTests);

            try
            {
                this.testsController.Import("1", this.file.Object, true);
            }
            catch (Exception)
            {
                remainingTests = this.Data.Tests.All().Where(test => test.ProblemId == 1).Count();
                Assert.AreEqual(0, remainingTests);
            }
        }

        [TestMethod]
        public void ImportActionShouldReturnProperRedirectAndMessageIfZipFileIsNotValid()
        {
            this.file.Setup(x => x.ContentLength).Returns(1);
            this.file.Setup(x => x.FileName).Returns("filename.zip");
            this.file.Setup(x => x.InputStream).Returns(new MemoryStream());

            var redirectResult = this.testsController.Import("1", this.file.Object, false) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);
            Assert.AreEqual(1, redirectResult.RouteValues["id"]);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("DangerMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["DangerMessage"];
            Assert.AreEqual("Zip файлът е повреден", tempDataMessage);
        }

        [TestMethod]
        public void ImportActionShouldAddTestsToProblemIfZipFileIsCorrect()
        {
            var zipFile = new ZipFile();

            var inputTest = "input";
            var outputTest = "output";

            zipFile.AddEntry("test.001.in.txt", inputTest);
            zipFile.AddEntry("test.001.out.txt", outputTest);

            var zipStream = new MemoryStream();
            zipFile.Save(zipStream);
            zipStream.Position = 0;

            this.file.Setup(x => x.ContentLength).Returns(1);
            this.file.Setup(x => x.FileName).Returns("file.zip");
            this.file.Setup(x => x.InputStream).Returns(zipStream);

            var redirectResult = this.testsController.Import("1", this.file.Object, false) as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);

            Assert.AreEqual("Problem", redirectResult.RouteValues["action"]);
            Assert.AreEqual(1, redirectResult.RouteValues["id"]);

            var tests = this.Data.Problems.All().First(pr => pr.Id == 1).Tests.Count;
            Assert.AreEqual(14, tests);

            var tempDataHasKey = this.testsController.TempData.ContainsKey("InfoMessage");
            Assert.IsTrue(tempDataHasKey);

            var tempDataMessage = this.testsController.TempData["InfoMessage"];
            Assert.AreEqual("Тестовете са добавени към задачата", tempDataMessage);
        }
    }
}
