namespace OJS.Workers.Tools.AntiCheat
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using OJS.Workers.Tools.AntiCheat.Contracts;

    public class SortTrimLinesAndRemoveBlankLinesVisitor : IDetectPlagiarismVisitor
    {
        public string Visit(string text)
        {
            var lines = new List<string>();
            using (var stringReader = new StringReader(text))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        lines.Add(line.Trim());
                    }
                }
            }

            lines.Sort();

            var stringBuilder = new StringBuilder();
            foreach (var line in lines)
            {
                stringBuilder.AppendLine(line);
            }

            return stringBuilder.ToString();
        }
    }
}
