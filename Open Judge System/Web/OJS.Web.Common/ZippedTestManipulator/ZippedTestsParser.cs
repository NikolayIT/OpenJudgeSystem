namespace OJS.Web.Common.ZippedTestManipulator
{
    using Ionic.Zip;
    using System;
    using System.IO;
    using System.Linq;
    using OJS.Data.Models;

    public class ZippedTestsManipulator
    {
        public static TestsParseResult Parse(Stream stream)
        {
            var zipFile = ZipFile.Read(stream);

            var result = new TestsParseResult();

            ExcractInAndSolFiles(zipFile, result);
            ExcractInAndOutFiles(zipFile, result);
            ExtractTxtFiles(zipFile, result);
            ExtractIOIFiles(zipFile, result);

            return result;
        }

        public static void AddTestsToProblem(Problem problem, TestsParseResult tests)
        {
            var lastTest = problem.Tests.OrderByDescending(x => x.OrderBy).FirstOrDefault();
            int count = 1;

            if (lastTest != null)
            {
                count = lastTest.OrderBy + 1;
            }

            for (int i = 0; i < tests.ZeroInputs.Count; i++)
            {
                problem.Tests.Add(new Test
                {
                    IsTrialTest = true,
                    OrderBy = count,
                    Problem = problem,
                    InputDataAsString = tests.ZeroInputs[i],
                    OutputDataAsString = tests.ZeroOutputs[i],
                });

                count++;
            }

            for (int i = 0; i < tests.Inputs.Count; i++)
            {
                problem.Tests.Add(new Test
                {
                    IsTrialTest = false,
                    OrderBy = count,
                    Problem = problem,
                    InputDataAsString = tests.Inputs[i],
                    OutputDataAsString = tests.Outputs[i]
                });

                count++;
            }
        }

        private static void ExcractInAndSolFiles(ZipFile zipFile, TestsParseResult result)
        {
            //.in and .sol files
            if (zipFile.Entries.Any(x => x.FileName.ToLower().Substring(x.FileName.Length - 4, 4) == ".sol"))
            {
                var solOutputs = zipFile.EntriesSorted.Where(x => x.FileName.ToLower().Substring(x.FileName.Length - 4, 4) == ".sol").ToList();

                foreach (var output in solOutputs)
                {
                    if (!zipFile.Entries.Any(x => x.FileName.ToLower().Contains((output.FileName.ToLower().Substring(0, output.FileName.Length - 3) + "in"))))
                    {
                        throw new ArgumentException("Невалидно име на входен тест");
                    }
                    else
                    {
                        var inputFiles = zipFile.Entries.Where(x => x.FileName.ToLower().Contains((output.FileName.ToLower().Substring(0, output.FileName.Length - 3) + "in"))).ToList();

                        if (inputFiles.Count > 1)
                        {
                            throw new ArgumentException("Невалиден брой входни тестове");
                        }

                        var input = inputFiles[0];

                        //check zero test
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
            //.in and .out files
            if (zipFile.Entries.Any(x => x.FileName.ToLower().Substring(x.FileName.Length - 4, 4) == ".out"))
            {
                var outOutputs = zipFile.EntriesSorted.Where(x => x.FileName.ToLower().Substring(x.FileName.Length - 4, 4) == ".out").ToList();

                foreach (var output in outOutputs)
                {
                    if (!zipFile.Entries.Any(x => x.FileName.ToLower().Contains((output.FileName.ToLower().Substring(0, output.FileName.Length - 3) + "in"))))
                    {
                        throw new ArgumentException("Невалидно име на входен тест");
                    }
                    else
                    {
                        var inputFiles = zipFile.Entries.Where(x => x.FileName.ToLower().Contains((output.FileName.ToLower().Substring(0, output.FileName.Length - 3) + "in"))).ToList();

                        if (inputFiles.Count > 1)
                        {
                            throw new ArgumentException("Невалиден брой входни тестове");
                        }

                        var input = inputFiles[0];

                        //check zero test
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
                    if (!zipFile.Entries.Any(x => x.FileName.ToLower().Contains((output.FileName.ToLower().Substring(0, output.FileName.Length - 7) + "in.txt"))))
                    {
                        throw new ArgumentException("Невалидно име на входен тест");
                    }
                    else
                    {
                        var inputFiles = zipFile.Entries.Where(x => x.FileName.ToLower().Contains((output.FileName.ToLower().Substring(0, output.FileName.Length - 7) + "in.txt"))).ToList();

                        if (inputFiles.Count > 1)
                        {
                            throw new ArgumentException("Невалиден брой входни тестове");
                        }

                        var input = inputFiles[0];

                        //check zero test
                        if (input.FileName.Contains(".000.")
                            && output.FileName.Contains(".000."))
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

        private static void ExtractIOIFiles(ZipFile zipFile, TestsParseResult result)
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
                    else
                    {
                        var inputFiles = zipFile.Entries.Where(x => x.FileName.ToLower().Contains("in" + output.FileName.ToLower().Substring(output.FileName.ToLower().LastIndexOf('.')))
                            && x.FileName.Substring(x.FileName.LastIndexOf('.')) == output.FileName.Substring(output.FileName.LastIndexOf('.'))).ToList();

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
        }

        private static string ExtractFileFromStream(ZipEntry entry)
        {
            MemoryStream reader = new MemoryStream();

            entry.Extract(reader);

            string text;

            using (reader)
            {
                reader.Position = 0;

                StreamReader sr = new StreamReader(reader);

                text = sr.ReadToEnd();
            }

            return text;
        }
    }
}
