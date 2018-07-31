namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.IO;
    using System.Numerics;
    using System.Text.RegularExpressions;
    using Nethereum.Hex.HexTypes;
    using Nethereum.Web3;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Compilers;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.Executors;

    public class SolidityCompileDeployAndRunUnitTestsExecutionStrategy : ExecutionStrategy
    {
        private const string AbiFileSearchPattern = "*.abi";
        private const string ContractNameRegexPattern = @"^\s*contract\s+([A-Za-z]\w*)\s*\{";
        private readonly string nodeJsExecutablePath;
        private readonly string ganacheNodeCliPath;
        private readonly string truffleExecutablePath;
        private readonly int portNumber;

        private readonly HexBigInteger gas = new HexBigInteger(new BigInteger(5000000m));

        public SolidityCompileDeployAndRunUnitTestsExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            string nodeJsExecutablePath,
            string ganacheNodeCliPath,
            string truffleExecutablePath,
            int portNumber,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed)
        {
            if (!File.Exists(nodeJsExecutablePath))
            {
                throw new ArgumentException(
                    $"NodeJS not found in: {nodeJsExecutablePath}",
                    nameof(nodeJsExecutablePath));
            }

            if (!File.Exists(ganacheNodeCliPath))
            {
                throw new ArgumentException(
                    $"Ganache-cli not found in: {ganacheNodeCliPath}",
                    nameof(ganacheNodeCliPath));
            }

            if (!File.Exists(truffleExecutablePath))
            {
                throw new ArgumentException(
                    $"Truffle not found in: {truffleExecutablePath}",
                    nameof(truffleExecutablePath));
            }

            this.nodeJsExecutablePath = nodeJsExecutablePath;
            this.ganacheNodeCliPath = ganacheNodeCliPath;
            this.truffleExecutablePath = truffleExecutablePath;
            this.portNumber = portNumber;
            this.GetCompilerPathFunc = getCompilerPathFunc;
        }

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            var contractName = Regex
                .Match(executionContext.Code, ContractNameRegexPattern, RegexOptions.Multiline)
                ?.Groups[1]
                ?.Value;

            if (string.IsNullOrEmpty(contractName))
            {
                throw new ArgumentException("No valid contract is found");
            }

            var truffle = new TruffleProjectManager(this.WorkingDirectory, this.portNumber);

            truffle.CreateContract(contractName, executionContext.Code);

            // Compile the file
            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);
            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                this.WorkingDirectory);

            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            //var(byteCode, abi) = GetByteCodeAndAbi(compilerResult.OutputFile);

            // Run in the Ethereum Virtual Machine scope
            using (new GanacheCliScope(this.nodeJsExecutablePath, this.ganacheNodeCliPath, this.portNumber))
            {
                //var web3 = new Web3($"http://localhost:{this.portNumber}");

                //var senderAddress = web3.Eth.Accounts.SendRequestAsync().GetAwaiter().GetResult()[0];

                //// Deploy the contract
                //var transactionHash = web3.Eth.DeployContract
                //    .SendRequestAsync(abi, byteCode, senderAddress, this.gas)
                //    .GetAwaiter()
                //    .GetResult();

                //var receipt = web3.Eth.Transactions.GetTransactionReceipt
                //    .SendRequestAsync(transactionHash)
                //    .GetAwaiter()
                //    .GetResult();

                //var contractAddress = receipt.ContractAddress;

                //var contract = web3.Eth.GetContract(abi, contractAddress);

                IExecutor executor = new StandardProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);

                // Run tests
                var processExecutionResult = executor.Execute(
                    this.truffleExecutablePath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    new[] { "test" },
                    this.WorkingDirectory);
            }

            throw new NotImplementedException();
        }

        protected override CompileResult Compile(
            CompilerType compilerType,
            string compilerPath,
            string compilerArguments,
            string submissionFilePath)
        {
            var compiler = Compiler.CreateCompiler(compilerType);
            var compilerResult = compiler.Compile(compilerPath, submissionFilePath, compilerArguments);

            return compilerResult;
        }

        private static(string byteCode, string abi) GetByteCodeAndAbi(string compilerResultOutputFile)
        {
            var byteCode = File.ReadAllText(compilerResultOutputFile);

            var abiFile = FileHelpers.FindFileMatchingPattern(
                Path.GetDirectoryName(compilerResultOutputFile),
                AbiFileSearchPattern);

            var abi = File.ReadAllText(abiFile);

            return (byteCode, abi);
        }
    }
}