namespace OJS.Workers.Common.Extensions
{
    using System;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        public static string MaxLength(this string stringToTrim, int maxLength)
        {
            if (stringToTrim == null || stringToTrim.Length <= maxLength)
            {
                return stringToTrim;
            }

            return stringToTrim.Substring(0, maxLength);
        }

        public static string GetStringWithEllipsisBetween(this string input, int startIndex, int endIndex)
        {
            string result = null;

            if (input != null)
            {
                if (startIndex == endIndex)
                {
                    result = string.Empty;
                }
                else
                {
                    const string Ellipsis = "...";

                    result = string.Format(
                        "{0}{1}{2}",
                        startIndex > Ellipsis.Length ? Ellipsis : string.Empty,
                        input.Substring(startIndex, endIndex - startIndex),
                        input.Length - endIndex > Ellipsis.Length ? Ellipsis : string.Empty);
                }
            }

            return result;
        }

        public static int GetFirstDifferenceIndexWith(this string input, string other, bool ignoreCase = false)
        {
            var firstDifferenceIndex = -1;

            if (input != null && other != null)
            {
                var maxIndex = Math.Min(input.Length, other.Length);
                for (var i = 0; i < maxIndex; i++)
                {
                    var areEqualChars = ignoreCase ?
                        char.ToUpperInvariant(input[i]) == char.ToUpperInvariant(other[i]) :
                        input[i] == other[i];
                    if (!areEqualChars)
                    {
                        firstDifferenceIndex = i;
                        break;
                    }
                }

                if (firstDifferenceIndex < 0 && input.Length != other.Length)
                {
                    firstDifferenceIndex = maxIndex;
                }
            }

            if (input == null ^ other == null)
            {
                firstDifferenceIndex = 0;
            }

            return firstDifferenceIndex;
        }

        public static string ToSingleLine(this string input) =>
            Regex.Replace(input, @"\t|\r|\n", string.Empty);

        public static string RemoveMultipleSpaces(this string input) =>
            Regex.Replace(input, @"\s+", " ");
    }
}