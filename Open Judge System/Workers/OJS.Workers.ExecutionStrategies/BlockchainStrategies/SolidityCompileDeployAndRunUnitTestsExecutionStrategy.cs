namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.IO;
    using System.Numerics;

    using Nethereum.Hex.HexTypes;
    using Nethereum.Web3;

    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class SolidityCompileDeployAndRunUnitTestsExecutionStrategy : ExecutionStrategy
    {
        private const string AbiFileSearchPattern = "*.abi";
        private readonly string ganacheExetuablePath;

        private readonly HexBigInteger gas = new HexBigInteger(new BigInteger(5000000m));

        public SolidityCompileDeployAndRunUnitTestsExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            string ganacheExetuablePath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed)
        {
            this.ganacheExetuablePath = ganacheExetuablePath;
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
            //using (var process = new Process())
            //{
            //    process.StartInfo = new ProcessStartInfo(@"C:\Windows\System32\cmd.exe")
            //    {
            //        Arguments = "/C " + this.ganacheExetuablePath,
            //        WindowStyle = ProcessWindowStyle.Hidden,
            //        UseShellExecute = true
            //    };

            //    process.Start();

            var web3 = new Web3();

            var senderAddress = web3.Eth.Accounts.SendRequestAsync().GetAwaiter().GetResult()[0];

            // Deploy the contract
            var transactionHash = web3.Eth.DeployContract
                .SendRequestAsync(abi, byteCode, senderAddress, this.gas, "Pesho")
                .GetAwaiter()
                .GetResult();

            var receipt = web3.Eth.Transactions.GetTransactionReceipt
                .SendRequestAsync(transactionHash)
                .GetAwaiter()
                .GetResult();

            var contractAddress = receipt.ContractAddress;

            var contract = web3.Eth.GetContract(abi, contractAddress);

            //IExecutor executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);

            //    process.CloseMainWindow();
            //}

            // Run tests
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