namespace OJS.Workers.Tools.AntiCheat.Contracts
{
    public interface IDetectPlagiarismVisitor
    {
        string Visit(string text);
    }
}
