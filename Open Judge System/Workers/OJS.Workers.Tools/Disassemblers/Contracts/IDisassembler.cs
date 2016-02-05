namespace OJS.Workers.Tools.Disassemblers.Contracts
{
    public interface IDisassembler
    {
        DisassembleResult Disassemble(string compiledFilePath, string additionalArguments = null);
    }
}
