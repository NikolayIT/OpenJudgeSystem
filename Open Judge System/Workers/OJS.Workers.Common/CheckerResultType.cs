namespace OJS.Workers.Common
{
    using System;

    [Flags]
    public enum CheckerResultType
    {
        Ok = 0,

        WrongAnswer = 1 << 0,

        InvalidOutputFormat = 1 << 1,

        InvalidNumberOfLines = 1 << 2,
    }
}
