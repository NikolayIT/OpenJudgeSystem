namespace OJS.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        public static DateTime TryGetDate(this string date)
        {
            try
            {
                return DateTime.ParseExact(date, "dd/MM/yyyy", null);
            }
            catch (Exception)
            {
                return new DateTime(2010, 1, 1);
            }
        }

        public static byte[] ToByteArray(this string sourceString)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(sourceString);
        }

        public static int ToInteger(this string input)
        {
            int integerValue;
            int.TryParse(input, out integerValue);
            return integerValue;
        }

        public static string ToUrl(this string uglyString)
        {
            var resultString = new StringBuilder(uglyString.Length);
            bool isLastCharacterDash = false;

            uglyString = uglyString.Replace("C#", "CSharp");
            uglyString = uglyString.Replace("C++", "CPlusPlus");

            foreach (var character in uglyString)
            {
                if (char.IsLetterOrDigit(character))
                {
                    resultString.Append(character);
                    isLastCharacterDash = false;
                }
                else if (!isLastCharacterDash)
                {
                    resultString.Append('-');
                    isLastCharacterDash = true;
                }
            }

            return resultString.ToString().Trim('-');
        }

        public static string Repeat(this string input, int count)
        {
            var builder = new StringBuilder((input?.Length ?? 0) * count);

            for (int i = 0; i < count; i++)
            {
                builder.Append(input);
            }

            return builder.ToString();
        }

        public static string MaxLength(this string stringToTrim, int maxLength)
        {
            if (stringToTrim == null || stringToTrim.Length <= maxLength)
            {
                return stringToTrim;
            }

            return stringToTrim.Substring(0, maxLength);
        }

        // TODO: Unit test
        public static string MaxLengthWithEllipsis(this string stringToTrim, int maxLength)
        {
            if (stringToTrim == null || stringToTrim.Length <= maxLength)
            {
                return stringToTrim;
            }

            return stringToTrim.Substring(0, maxLength) + "...";
        }

        // TODO: Test
        public static IEnumerable<string> GetStringsBetween(this string stringToParse, string beforeString, string afterString)
        {
            var regEx = new Regex(Regex.Escape(beforeString) + "(.*?)" + Regex.Escape(afterString), RegexOptions.Singleline | RegexOptions.Compiled);
            var matches = regEx.Matches(stringToParse);
            foreach (Match match in matches)
            {
                yield return match.Groups[1].Value;
            }
        }

        // TODO: Test
        public static string GetStringBetween(this string stringToParse, string beforeString, string afterString)
        {
            var strings = stringToParse.GetStringsBetween(beforeString, afterString).ToList();
            if (!strings.Any())
            {
                return null;
            }

            return strings[0];
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

        // TODO: Test
        public static string ToValidFileName(this string input)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars();
            var fixedString = new StringBuilder(input);
            foreach (var ch in invalidCharacters)
            {
                fixedString.Replace(ch, '_');
            }

            return fixedString.ToString();
        }

        // TODO: Test
        public static string ToValidFilePath(this string input)
        {
            var invalidCharacters = Path.GetInvalidPathChars();
            var fixedString = new StringBuilder(input);
            foreach (var ch in invalidCharacters)
            {
                fixedString.Replace(ch, '_');
            }

            return fixedString.ToString();
        }

        public static string PascalCaseToText(this string input)
        {
            if (input == null)
            {
                return null;
            }

            const char WhiteSpace = ' ';

            var result = new StringBuilder();
            var currentWord = new StringBuilder();
            var abbreviation = new StringBuilder();

            char previous = WhiteSpace;
            bool inWord = false;
            bool isAbbreviation = false;

            for (int i = 0; i < input.Length; i++)
            {
                char symbolToAdd = input[i];

                if (char.IsUpper(symbolToAdd) && previous == WhiteSpace && !inWord)
                {
                    inWord = true;
                    isAbbreviation = true;
                    abbreviation.Append(symbolToAdd);
                }
                else if (char.IsUpper(symbolToAdd) && inWord)
                {
                    abbreviation.Append(symbolToAdd);
                    currentWord.Append(WhiteSpace);
                    symbolToAdd = char.ToLower(symbolToAdd);
                }
                else if (char.IsLower(symbolToAdd) && inWord)
                {
                    isAbbreviation = false;
                }
                else if (symbolToAdd == WhiteSpace)
                {
                    result.Append(isAbbreviation && abbreviation.Length > 1 ? abbreviation.ToString() : currentWord.ToString());
                    currentWord.Clear();
                    abbreviation.Clear();

                    if (result.Length > 0)
                    {
                        abbreviation.Append(WhiteSpace);
                    }

                    inWord = false;
                    isAbbreviation = false;
                }

                previous = symbolToAdd;
                currentWord.Append(symbolToAdd);
            }

            if (currentWord.Length > 0)
            {
                result.Append(isAbbreviation && abbreviation.Length > 1 ? abbreviation.ToString() : currentWord.ToString());
            }

            return result.ToString();
        }

        public static string ToSingleLine(this string input) =>
            Regex.Replace(input, @"\t|\r|\n", string.Empty);

        public static string RemoveMultipleSpaces(this string input) =>
            Regex.Replace(input, @"\s+", " ");
    }
}
