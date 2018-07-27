namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Numerics;
    using System.Threading;
    using Nethereum.Hex.HexTypes;
    using Nethereum.Web3;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.Executors;

    using ExecutionContext = OJS.Workers.ExecutionStrategies.ExecutionContext;

    public class SolidityCompileDeployAndRunUnitTestsExecutionStrategy : ExecutionStrategy
    {
        private const string AbiFileSearchPattern = "*.abi";
        private readonly string nodeJsExecutablePath;
        private readonly string ganacheNodeCliPath;
        private readonly string truffleExecutablePath;

        private readonly HexBigInteger gas = new HexBigInteger(new BigInteger(5000000m));

        public SolidityCompileDeployAndRunUnitTestsExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            string nodeJsExecutablePath,
            string ganacheNodeCliPath,
            string truffleExecutablePath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed)
        {
            this.nodeJsExecutablePath = nodeJsExecutablePath;
            this.ganacheNodeCliPath = ganacheNodeCliPath;
            this.truffleExecutablePath = truffleExecutablePath;
            this.GetCompilerPathFunc = getCompilerPathFunc;
        }

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            // Compile the file
            var compilerResult = this.ExecuteCompiling(executionContext, this.GetCompilerPathFunc, result);
            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var(byteCode, abi) = GetByteCodeAndAbi(compilerResult.OutputFile);

            // Run the EVM
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(this.nodeJsExecutablePath)
                {
                    Arguments = this.ganacheNodeCliPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                process.Start();

                Thread.Sleep(1500);

                var web3 = new Web3();

                var senderAddress = web3.Eth.Accounts.SendRequestAsync().GetAwaiter().GetResult()[0];

                // Deploy the contract
                var transactionHash = web3.Eth.DeployContract
                    .SendRequestAsync(abi, byteCode, senderAddress, this.gas)
                    .GetAwaiter()
                    .GetResult();

                var receipt = web3.Eth.Transactions.GetTransactionReceipt
                    .SendRequestAsync(transactionHash)
                    .GetAwaiter()
                    .GetResult();

                var contractAddress = receipt.ContractAddress;

                var contract = web3.Eth.GetContract(abi, contractAddress);

                IExecutor executor = new StandardProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);

                var processExecutionResult = executor.Execute(
                    this.truffleExecutablePath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    new[] { "test" },
                    this.WorkingDirectory);

                /*Run tests here*/

                if (!process.HasExited)
                {
                    process.Kill();
                }
            }

            throw new NotImplementedException();
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