namespace OJS.Web.Common.ZippedTestManipulator
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Ionic.Zip;

    using OJS.Common;
    using OJS.Data.Models;
    using OJS.Web.Common.Extensions;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;

    public class ZippedTestsParser
    {
        private const string InvalidNameForInputTestErrorMessage = "Невалидно име на входен тест";
        private const string InvalidInputTestsCountErrorMessage = "Невалиден брой входни тестове";

        public static TestsParseResult Parse(Stream stream)
        {
            var zipFile = ZipFile.Read(stream);

            var result = new TestsParseResult();

            ExcractInAndSolFiles(zipFile, result);
            ExcractInAndOutFiles(zipFile, result);
            ExtractTxtFiles(zipFile, result);
            ExtractIoiFiles(zipFile, result);
            ExtractZipFiles(zipFile, result);

            return result;
        }

        public static int AddTestsToProblem(Problem problem, TestsParseResult tests)
        {
            var lastTrialTest = problem.Tests
                .Where(x => x.IsTrialTest)
                .OrderByDescending(x => x.OrderBy)
                .FirstOrDefault();

            var zeroTestsOrder = 1;

            if (lastTrialTest != null)
            {
                zeroTestsOrder = lastTrialTest.OrderBy + 1;
            }

            var addedTestsCount = 0;

            for (var i = 0; i < tests.ZeroInputs.Count; i++)
            {
                problem.Tests.Add(new Test
                {
                    IsTrialTest = true,
                    OrderBy = zeroTestsOrder,
                    Problem = problem,
                    InputDataAsString = tests.ZeroInputs[i],
                    OutputDataAsString = tests.ZeroOutputs[i],
                });

                zeroTestsOrder++;
                addedTestsCount++;
            }

            var lastTest = problem.Tests
                .Where(x => !x.IsTrialTest)
                .OrderByDescending(x => x.OrderBy)
                .FirstOrDefault();

            var orderBy = 1;

            if (lastTest != null)
            {
                orderBy = lastTest.OrderBy + 1;
            }

            for (var i = 0; i < tests.OpenInputs.Count; i++)
            {
                problem.Tests.Add(new Test
                {
                    IsTrialTest = false,
                    IsOpenTest = true,
                    OrderBy = orderBy,
                    Problem = problem,
                    InputDataAsString = tests.OpenInputs[i],
                    OutputDataAsString = tests.OpenOutputs[i]
                });

                orderBy++;
                addedTestsCount++;
            }

            for (var i = 0; i < tests.Inputs.Count; i++)
            {
                problem.Tests.Add(new Test
                {
                    IsTrialTest = false,
                    OrderBy = orderBy,
                    Problem = problem,
                    InputDataAsString = tests.Inputs[i],
                    OutputDataAsString = tests.Outputs[i]
                });

                orderBy++;
                addedTestsCount++;
            }

            return addedTestsCount;
        }

        public static string ExtractFileFromStream(ZipEntry entry)
        {
            using (var memoryStream = new MemoryStream())
            {
                entry.Extract(memoryStream);

                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                {
                    var text = streamReader.ReadToEnd();
                    return text;
                }
            }
        }

        public static bool AreTestsParsedCorrectly(TestsParseResult tests)
        {
            var hasInputs = tests.ZeroInputs.Count != 0 ||
                tests.Inputs.Count != 0 ||
                tests.OpenInputs.Count != 0;

            var hasEqualAmountOfInputsAndOutputs = tests.ZeroInputs.Count == tests.ZeroOutputs.Count &&
                tests.Inputs.Count == tests.Outputs.Count &&
                tests.OpenInputs.Count == tests.OpenOutputs.Count;

            return hasInputs && hasEqualAmountOfInputsAndOutputs;
        }

        private static void ExcractInAndSolFiles(ZipFile zipFile, TestsParseResult result)
        {
            var solOutputs = zipFile.GetZipEntriesByExtensions(GlobalConstants.SolFileExtension);

            foreach (var output in solOutputs)
            {
                var input = GetInputByOutputAndExtension(zipFile, output, GlobalConstants.InputFileExtension);

                var zeroTestInputSignature = $"00{GlobalConstants.InputFileExtension}";
                var zeroTestOutputSignature = $"00{GlobalConstants.SolFileExtension}";

                var isZeroTest =
                    input.FileName
                        .Substring(
                            input.FileName.Length - zeroTestInputSignature.Length,
                            zeroTestInputSignature.Length)
                        .Equals(zeroTestInputSignature, StringComparison.OrdinalIgnoreCase) &&
                    output.FileName
                        .Substring(
                            output.FileName.Length - zeroTestOutputSignature.Length,
                            zeroTestOutputSignature.Length)
                        .Equals(zeroTestOutputSignature, StringComparison.OrdinalIgnoreCase);

                if (isZeroTest)
                {
                    result.ZeroInputs.Add(ExtractFileFromStream(input));
                    result.ZeroOutputs.Add(ExtractFileFromStream(output));
                }
                else
                {
                    result.Inputs.Add(ExtractFileFromStream(input));
                    result.Outputs.Add(ExtractFileFromStream(output));
                }
            }
        }

        private static void ExcractInAndOutFiles(ZipFile zipFile, TestsParseResult result)
        {
            var outOutputs = zipFile.GetZipEntriesByExtensions(GlobalConstants.OutputFileExtension);

            foreach (var output in outOutputs)
            {
                var input = GetInputByOutputAndExtension(zipFile, output, GlobalConstants.InputFileExtension);

                const string zeroTestSignature = "et";

                var isZeroTest =
                    input.FileName
                        .Substring(
                            input.FileName.LastIndexOf('_') - zeroTestSignature.Length,
                            zeroTestSignature.Length)
                        .Equals(zeroTestSignature, StringComparison.OrdinalIgnoreCase) &&
                    output.FileName
                        .Substring(
                            output.FileName.LastIndexOf('_') - zeroTestSignature.Length,
                            zeroTestSignature.Length)
                        .Equals(zeroTestSignature, StringComparison.OrdinalIgnoreCase);

                if (isZeroTest)
                {
                    result.ZeroInputs.Add(ExtractFileFromStream(input));
                    result.ZeroOutputs.Add(ExtractFileFromStream(output));
                }
                else
                {
                    result.Inputs.Add(ExtractFileFromStream(input));
                    result.Outputs.Add(ExtractFileFromStream(output));
                }
            }
        }

        private static void ExtractTxtFiles(ZipFile zipFile, TestsParseResult result)
        {
            var outOutputs = zipFile.GetZipEntriesByExtensions(WebConstants.TestOutputTxtFileExtension);

            foreach (var output in outOutputs)
            {
                var input = GetInputByOutputAndExtension(zipFile, output, WebConstants.TestInputTxtFileExtension);

                if (IsStandardZeroTest(input, output))
                {
                    result.ZeroInputs.Add(ExtractFileFromStream(input));
                    result.ZeroOutputs.Add(ExtractFileFromStream(output));
                }
                else if (IsStandardOpenTest(input, output))
                {
                    result.OpenInputs.Add(ExtractFileFromStream(input));
                    result.OpenOutputs.Add(ExtractFileFromStream(output));
                }
                else
                {
                    result.Inputs.Add(ExtractFileFromStream(input));
                    result.Outputs.Add(ExtractFileFromStream(output));
                }
            }
        }

        private static void ExtractIoiFiles(ZipFile zipFile, TestsParseResult result)
        {
            // IOI test files
            if (zipFile.Entries.Any(x => char.IsNumber(x.FileName[x.FileName.LastIndexOf('.') + 1])))
            {
                const string outputFileSignature = "out";
                const string inputFileSignature = "in";

                var outOutputs = zipFile.EntriesSorted
                    .Where(x => x.FileName
                        .Substring(
                            x.FileName.LastIndexOf('.') - outputFileSignature.Length,
                            outputFileSignature.Length)
                        .Equals(outputFileSignature, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var output in outOutputs)
                {
                    var inputFileName = inputFileSignature +
                        output.FileName.Substring(output.FileName.LastIndexOf('.'));

                    var input = GetUniqueInputByFileName(zipFile, inputFileName);

                    result.Inputs.Add(ExtractFileFromStream(input));
                    result.Outputs.Add(ExtractFileFromStream(output));
                }
            }
        }

        private static void ExtractZipFiles(ZipFile zipFile, TestsParseResult result)
        {
            // Java Unit Testing test files
            var outputs = zipFile.GetZipEntriesByExtensions(WebConstants.TestOutputZipFileExtension).ToList();

            if (!outputs.Any())
            {
                return;
            }

            var tempDir = DirectoryHelpers.CreateTempDirectory();

            foreach (var output in outputs)
            {
                var input = GetInputByOutputAndExtension(zipFile, output, WebConstants.TestInputZipFileExtension);

                var inputAsText = new StringBuilder();
                var outputAsText = new StringBuilder();

                input.Extract(tempDir);
                output.Extract(tempDir);

                var inputFile = ZipFile.Read($"{tempDir}\\{input.FileName}");
                var outputFile = ZipFile.Read($"{tempDir}\\{output.FileName}");

                var inputEntries = inputFile.Entries.Where(x => !x.FileName.EndsWith("/"));
                var outputEntries = outputFile.Entries.Where(x => !x.FileName.EndsWith("/"));

                foreach (var entry in inputEntries)
                {
                    inputAsText.Append(Constants.ClassDelimiter);
                    inputAsText.AppendLine($"//{entry.FileName}");
                    inputAsText.AppendLine(ExtractFileFromStream(entry));
                }

                foreach (var entry in outputEntries)
                {
                    outputAsText.AppendLine(ExtractFileFromStream(entry));
                }

                inputFile.Dispose();
                outputFile.Dispose();

                if (IsStandardZeroTest(input, output))
                {
                    result.ZeroInputs.Add(inputAsText.ToString());
                    result.ZeroOutputs.Add(outputAsText.ToString());
                }
                else if (IsStandardOpenTest(input, output))
                {
                    result.OpenInputs.Add(inputAsText.ToString());
                    result.OpenOutputs.Add(outputAsText.ToString());
                }
                else
                {
                    result.Inputs.Add(inputAsText.ToString());
                    result.Outputs.Add(outputAsText.ToString());
                }
            }

            DirectoryHelpers.SafeDeleteDirectory(tempDir, true);
        }

        private static ZipEntry GetInputByOutputAndExtension(
            ZipFile zipFile,
            ZipEntry output,
            string extension)
        {
            var fileName = output.FileName
                .Substring(0, output.FileName.Length - extension.Length - 1) + extension;

            return GetUniqueInputByFileName(zipFile, fileName);
        }

        private static ZipEntry GetUniqueInputByFileName(ZipFile zipFile, string fileName)
        {
            var files = zipFile.Entries
                .Where(x => x.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (files.Count == 0)
            {
                throw new ArgumentException(InvalidNameForInputTestErrorMessage);
            }

            if (files.Count > 1)
            {
                throw new ArgumentException(InvalidInputTestsCountErrorMessage);
            }

            return files.First();
        }

        private static bool IsStandardZeroTest(ZipEntry input, ZipEntry output) =>
            input.FileName.Contains(WebConstants.ZeroTestStandardSignature) &&
            output.FileName.Contains(WebConstants.ZeroTestStandardSignature);

        private static bool IsStandardOpenTest(ZipEntry input, ZipEntry output) =>
            input.FileName.Contains(WebConstants.OpenTestStandardSignature) &&
            output.FileName.Contains(WebConstants.OpenTestStandardSignature);
    }
}