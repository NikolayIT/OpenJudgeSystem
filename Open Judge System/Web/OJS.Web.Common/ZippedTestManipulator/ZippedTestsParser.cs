namespace OJS.Web.Common.ZippedTestManipulator
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Ionic.Zip;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Data.Models;

    public class ZippedTestsParser
    {
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
            var lastTrialTest = problem.Tests.Where(x => x.IsTrialTest).OrderByDescending(x => x.OrderBy).FirstOrDefault();
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

            var lastTest = problem.Tests.Where(x => !x.IsTrialTest).OrderByDescending(x => x.OrderBy).FirstOrDefault();
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
            var reader = new MemoryStream();
            entry.Extract(reader);
            reader = new MemoryStream(reader.ToArray());

            var streamReader = new StreamReader(reader);

            var text = streamReader.ReadToEnd();
            reader.Dispose();
            return text;
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
            // .in and .sol files
            if (zipFile.Entries.Any(x => x.FileName.ToLower().Substring(x.FileName.Length - 4, 4) == ".sol"))
            {
                var solOutputs = zipFile.EntriesSorted.Where(x => x.FileName.ToLower().Substring(x.FileName.Length - 4, 4) == ".sol").ToList();

                foreach (var output in solOutputs)
                {
                    if (!zipFile.Entries.Any(x => x.FileName.ToLower().Contains(output.FileName.ToLower().Substring(0, output.FileName.Length - 3) + "in")))
                    {
                        throw new ArgumentException("Невалидно име на входен тест");
                    }
                    else
                    {
                        var inputFiles = zipFile.Entries.Where(x => x.FileName.ToLower().Contains(output.FileName.ToLower().Substring(0, output.FileName.Length - 3) + "in")).ToList();

                        if (inputFiles.Count > 1)
                        {
                            throw new ArgumentException("Невалиден брой входни тестове");
                        }

                        var input = inputFiles[0];

                        // check zero test
                        if (input.FileName.ToLower().Substring(input.FileName.Length - 5, 5) == "00.in"
                            && output.FileName.ToLower().Substring(output.FileName.Length - 6, 6) == "00.sol")
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
            }
        }

        private static void ExcractInAndOutFiles(ZipFile zipFile, TestsParseResult result)
        {
            // .in and .out files
            if (zipFile.Entries.Any(x => x.FileName.ToLower().Substring(x.FileName.Length - 4, 4) == ".out"))
            {
                var outOutputs = zipFile.EntriesSorted.Where(x => x.FileName.ToLower().Substring(x.FileName.Length - 4, 4) == ".out").ToList();

                foreach (var output in outOutputs)
                {
                    if (!zipFile.Entries.Any(x => x.FileName.ToLower().Contains(output.FileName.ToLower().Substring(0, output.FileName.Length - 3) + "in")))
                    {
                        throw new ArgumentException("Невалидно име на входен тест");
                    }
                    else
                    {
                        var inputFiles = zipFile.Entries.Where(x => x.FileName.ToLower().Contains(output.FileName.ToLower().Substring(0, output.FileName.Length - 3) + "in")).ToList();

                        if (inputFiles.Count > 1)
                        {
                            throw new ArgumentException("Невалиден брой входни тестове");
                        }

                        var input = inputFiles[0];

                        // check zero test
                        if (input.FileName.ToLower().Substring(input.FileName.LastIndexOf('_') - 2, 2) == "et"
                            && output.FileName.ToLower().Substring(output.FileName.LastIndexOf('_') - 2, 2) == "et")
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
            }
        }

        private static void ExtractTxtFiles(ZipFile zipFile, TestsParseResult result)
        {
            // bgcoder text files
            if (zipFile.Entries.Any(x => x.FileName.ToLower().Substring(x.FileName.Length - 8, 8) == ".out.txt"))
            {
                var outOutputs = zipFile.EntriesSorted.Where(x => x.FileName.ToLower().Substring(x.FileName.Length - 8, 8) == ".out.txt").ToList();

                foreach (var output in outOutputs)
                {
                    if (!zipFile.Entries.Any(x => x.FileName.ToLower().Contains(output.FileName.ToLower().Substring(0, output.FileName.Length - 7) + "in.txt")))
                    {
                        throw new ArgumentException("Невалидно име на входен тест");
                    }
                    else
                    {
                        var inputFiles = zipFile.Entries.Where(x => x.FileName.ToLower().Contains(output.FileName.ToLower().Substring(0, output.FileName.Length - 7) + "in.txt")).ToList();

                        if (inputFiles.Count > 1)
                        {
                            throw new ArgumentException("Невалиден брой входни тестове");
                        }

                        var input = inputFiles[0];

                        // check zero test
                        if (input.FileName.Contains(".000.")
                            && output.FileName.Contains(".000."))
                        {
                            result.ZeroInputs.Add(ExtractFileFromStream(input));
                            result.ZeroOutputs.Add(ExtractFileFromStream(output));
                        }
                        else if (input.FileName.Contains(".open.")
                            && output.FileName.Contains(".open."))
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
            }
        }

        private static void ExtractIoiFiles(ZipFile zipFile, TestsParseResult result)
        {
            // IOI test files
            if (zipFile.Entries.Any(x => char.IsNumber(x.FileName[x.FileName.ToLower().LastIndexOf('.') + 1])))
            {
                var outOutputs = zipFile.EntriesSorted.Where(x => x.FileName.ToLower().Substring(x.FileName.LastIndexOf('.') - 3, 3) == "out").ToList();

                foreach (var output in outOutputs)
                {
                    if (!zipFile.Entries.Any(x => x.FileName.ToLower().Contains("in" + x.FileName.ToLower().Substring(x.FileName.ToLower().LastIndexOf('.')))))
                    {
                        throw new ArgumentException("Невалидно име на входен тест");
                    }

                    var inputFiles = zipFile.Entries
                        .Where(x => x.FileName.ToLower()
                            .Contains("in" + output.FileName.ToLower()
                            .Substring(output.FileName.ToLower().LastIndexOf('.'))) &&
                            x.FileName.Substring(x.FileName.LastIndexOf('.')) == output.FileName.Substring(output.FileName.LastIndexOf('.')))
                        .ToList();

                    if (inputFiles.Count > 1)
                    {
                        throw new ArgumentException("Невалиден брой входни тестове");
                    }

                    var input = inputFiles[0];

                    result.Inputs.Add(ExtractFileFromStream(input));
                    result.Outputs.Add(ExtractFileFromStream(output));
                }
            }
        }

        private static void ExtractZipFiles(ZipFile zipFile, TestsParseResult result)
        {
            // Java Unit Testing test files
            if (zipFile.Entries.Any(x => x.FileName.ToLower().Substring(x.FileName.Length - 7, 7) == ".in.zip"))
            {
                var tempDir = DirectoryHelpers.CreateTempDirectory();
                var inputs = zipFile.EntriesSorted
                    .Where(x => x.FileName.ToLower().Substring(x.FileName.Length - 7, 7) == ".in.zip")
                    .ToList();

                foreach (var input in inputs)
                {
                    if (!zipFile.Entries
                        .Any(x => x.FileName
                            .ToLower()
                            .Contains(input.FileName
                                .ToLower()
                                .Substring(0, input.FileName.Length - 6) + "out.zip")))
                    {
                        throw new ArgumentException("Невалидно име на входен тест");
                    }

                    var outputFiles = zipFile.Entries
                        .Where(x => x.FileName
                            .ToLower()
                            .Contains(input.FileName
                                .ToLower()
                                .Substring(0, input.FileName.Length - 6) + "out.zip"))
                        .ToList();

                    if (outputFiles.Count > 1)
                    {
                        throw new ArgumentException("Невалиден брой входни тестове");
                    }

                    var output = outputFiles[0];
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
                        inputAsText.Append(GlobalConstants.ClassDelimiter);
                        inputAsText.AppendLine($"//{entry.FileName}");
                        inputAsText.AppendLine(ExtractFileFromStream(entry));
                    }

                    foreach (var entry in outputEntries)
                    {
                        outputAsText.AppendLine(ExtractFileFromStream(entry));
                    }

                    inputFile.Dispose();
                    outputFile.Dispose();

                    // check zero test
                    if (input.FileName.Contains(".000.")
                        && output.FileName.Contains(".000."))
                    {
                        result.ZeroInputs.Add(inputAsText.ToString());
                        result.ZeroOutputs.Add(outputAsText.ToString());
                    }
                    else if (input.FileName.Contains(".open.")
                        && output.FileName.Contains(".open."))
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
        }
    }
}
