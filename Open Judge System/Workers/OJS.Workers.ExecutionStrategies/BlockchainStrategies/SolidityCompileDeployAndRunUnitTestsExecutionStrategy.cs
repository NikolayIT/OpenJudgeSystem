namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Nethereum.Web3;

    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class SolidityCompileDeployAndRunUnitTestsExecutionStrategy : ExecutionStrategy
    {
        private const string AbiFileSearchPattern = "*.abi";
        private readonly string ganacheExetuablePath;

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

                var privateKey = "0xf1aa80d03be2392ae9b33159d497d9274e4ba5fea27bbc4aa2a38cdbbc4be172";
                var password = "passowrd";

                var senderAddress = web3.Eth.Accounts.SendRequestAsync().GetAwaiter().GetResult()[0];
                var balance = web3.Eth.GetBalance.SendRequestAsync(senderAddress).GetAwaiter().GetResult();

                var unlocked = web3.Personal.UnlockAccount.SendRequestAsync(senderAddress, password, 120).GetAwaiter().GetResult();

                //// Deploy the contract
                var deployedContract = web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, senderAddress, int.MaxValue).GetAwaiter().GetResult();

                //IExecutor executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);

            //    process.CloseMainWindow();
            //}

            // Run tests
            throw new NotImplementedException();
        }

        private static (string byteCode, string abi) GetByteCodeAndAbi(string compilerResultOutputFile)
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