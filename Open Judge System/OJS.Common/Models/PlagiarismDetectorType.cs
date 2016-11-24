namespace OJS.Common.Models
{
    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum PlagiarismDetectorType
    {
        [LocalizedDescription("CSharpCompileDisassemble", typeof(Resource))]
        CSharpCompileDisassemble = 1,

        [LocalizedDescription("JavaCompileDisassemble", typeof(Resource))]
        JavaCompileDisassemble = 2,

        [LocalizedDescription("PlainText", typeof(Resource))]
        PlainText = 3,
    }
}
