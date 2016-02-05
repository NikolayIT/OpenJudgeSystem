namespace OJS.Workers.Tools.AntiCheat
{
    using System.ComponentModel;

    public enum PlagiarismDetectorType
    {
        [Description("C# код")]
        CSharpCompileDisassemble = 1,

        [Description("Java код")]
        JavaCompileDisassemble = 2,

        [Description("Текст")]
        PlainText = 3,
    }
}
