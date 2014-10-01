namespace OJS.Workers.Tools.AntiCheat
{
    public interface IDetectPlagiarismVisitor
    {
        string Visit(string text);
    }
}
