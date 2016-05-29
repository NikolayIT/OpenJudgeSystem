namespace OJS.Workers.Tools.Disassemblers
{
    public class DisassembleResult
    {
        public DisassembleResult(bool isDisassembledSuccessfully, string disassembledCode = null)
        {
            this.IsDisassembledSuccessfully = isDisassembledSuccessfully;
            this.DisassembledCode = disassembledCode;
        }

        public bool IsDisassembledSuccessfully { get; set; }

        public string DisassembledCode { get; set; }
    }
}
