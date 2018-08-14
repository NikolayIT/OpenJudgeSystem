namespace OJS.Web.Common
{
    using OJS.Common;

    public static class WebConstants
    {
        public const string IsDisabled = "IsDisabled";

        // html attributes
        public const string Placeholder = "placeholder";

        // Tests
        public const string TestInputZipFileExtension =
            GlobalConstants.InputFileExtension +
            GlobalConstants.ZipFileExtension;

        public const string TestOutputZipFileExtension =
            GlobalConstants.OutputFileExtension +
            GlobalConstants.ZipFileExtension;

        public const string TestInputTxtFileExtension =
            GlobalConstants.InputFileExtension +
            GlobalConstants.TxtFileExtension;

        public const string TestOutputTxtFileExtension =
            GlobalConstants.OutputFileExtension +
            GlobalConstants.TxtFileExtension;

        public const string ZeroTestStandardSignature = ".000.";
        public const string OpenTestStandardSignature = ".open.";
    }
}