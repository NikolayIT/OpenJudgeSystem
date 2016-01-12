namespace OJS.Workers.Tools.Tests.Similarity
{
    using System.Collections.Generic;
    using System.Text;

    using NUnit.Framework;

    using OJS.Workers.Tools.Similarity;

    /// <summary>
    /// Tests are adapted from the original code (http://www.mathertel.de/Diff/)
    /// </summary>
    [TestFixture]
    public class SimilarityFinderDiffTextTests
    {
        [Test]
        public void TestWhenAllLinesAreDifferent()
        {
            var firstText = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            var secondText = "0,1,2,3,4,5,6,7,8,9".Replace(',', '\n');
            Assert.AreEqual("12.10.0.0*", TestHelper(new SimilarityFinder().DiffText(firstText, secondText, false, false, false)));
        }

        [Test]
        public void TestWhenAllLinesAreTheSame()
        {
            const string TextForSplit = "a,b,c,d,e,f,g,h,i,j,k,l";
            var firstText = TextForSplit.Replace(',', '\n');
            var secondText = TextForSplit.Replace(',', '\n');
            Assert.AreEqual(string.Empty, TestHelper(new SimilarityFinder().DiffText(firstText, secondText, false, false, false)));
        }

        [Test]
        public void TestWhenSnakeCase()
        {
            var firstText = "a,b,c,d,e,f".Replace(',', '\n');
            var secondText = "b,c,d,e,f,x".Replace(',', '\n');
            Assert.AreEqual("1.0.0.0*0.1.6.5*", TestHelper(new SimilarityFinder().DiffText(firstText, secondText, false, false, false)));
        }

        [Test]
        public void TestOneChangeWithinLongChainOfRepeats()
        {
            var firstText = "a,a,a,a,a,a,a,a,a,a".Replace(',', '\n');
            var secondText = "a,a,a,a,-,a,a,a,a,a".Replace(',', '\n');
            Assert.AreEqual("0.1.4.4*1.0.9.10*", TestHelper(new SimilarityFinder().DiffText(firstText, secondText, false, false, false)));
        }

        [Test]
        public void TestSomeDifferences()
        {
            var firstText = "a,b,-,c,d,e,f,f".Replace(',', '\n');
            var secondText = "a,b,x,c,e,f".Replace(',', '\n');
            Assert.AreEqual("1.1.2.2*1.0.4.4*1.0.7.6*", TestHelper(new SimilarityFinder().DiffText(firstText, secondText, false, false, false)));
        }

        [Test]
        public void TestReproduceScenario20020920()
        {
            var firstText = "c1,a,c2,b,c,d,e,g,h,i,j,c3,k,l".Replace(',', '\n');
            var secondText = "C1,a,C2,b,c,d,e,I1,e,g,h,i,j,C3,k,I2,l".Replace(',', '\n');
            Assert.AreEqual("1.1.0.0*1.1.2.2*0.2.7.7*1.1.11.13*0.1.13.15*", TestHelper(new SimilarityFinder().DiffText(firstText, secondText, false, false, false)));
        }

        [Test]
        public void TestReproduceScenario20030207()
        {
            var firstText = "F".Replace(',', '\n');
            var secondText = "0,F,1,2,3,4,5,6,7".Replace(',', '\n');
            Assert.AreEqual("0.1.0.0*0.7.1.2*", TestHelper(new SimilarityFinder().DiffText(firstText, secondText, false, false, false)));
        }

        [Test]
        public void TestReproduceScenarioMuegel()
        {
            var firstText = "HELLO\nWORLD".Replace(',', '\n');
            var secondText = "\n\nhello\n\n\n\nworld\n".Replace(',', '\n');
            Assert.AreEqual("2.8.0.0*", TestHelper(new SimilarityFinder().DiffText(firstText, secondText, false, false, false)));
        }

        [Test]
        public void TestCaseInsensitiveWithTrimLines()
        {
            var firstText = "HELLO\nWORLD ".Replace(',', '\n');
            var secondText = " hello \n world ".Replace(',', '\n');
            Assert.AreEqual(string.Empty, TestHelper(new SimilarityFinder().DiffText(firstText, secondText, true, true, true)));
        }

        private static string TestHelper(IEnumerable<Difference> differences)
        {
            var ret = new StringBuilder();
            foreach (var difference in differences)
            {
                ret.Append(difference.DeletedA + "." + difference.InsertedB + "." + difference.StartA + "." + difference.StartB + "*");
            }

            return ret.ToString();
        }
    }
}
