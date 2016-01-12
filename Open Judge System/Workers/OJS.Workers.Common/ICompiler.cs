namespace OJS.Workers.Common
{
    public interface ICompiler
    {
        /// <summary>
        /// Compiles given source code to binary code.
        /// </summary>
        /// <param name="compilerPath">Path to the compiler</param>
        /// <param name="inputFile">Source code given as a text file with no extension</param>
        /// <param name="additionalArguments">Additional compiler arguments</param>
        /// <returns>Result of compilation</returns>
        CompileResult Compile(string compilerPath, string inputFile, string additionalArguments);
    }
}
