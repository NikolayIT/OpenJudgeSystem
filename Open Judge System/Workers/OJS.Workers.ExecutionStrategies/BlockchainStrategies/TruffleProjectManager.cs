namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using OJS.Workers.Executors;

    using static OJS.Workers.Common.Constants;

    internal class TruffleProjectManager
    {
        public const string TestFileNamePrefix = "Test";

        private const string MigrationsContractName = "Migrations";
        private const string MigrationsFileName = MigrationsContractName + SolidityFileExtension;
        private const string ConfigFileName = "truffle-config";
        private const string TestsFolderName = "test";
        private const string MigrationsFolderName = "migrations";
        private const string ContractsFolderName = "contracts";
        private const string BuildsFolderName = "build";
        private const string ContractNamePlaceholder = "#contractName#";
        private const string AbiPlaceholder = "#abi#";
        private const string ByteCodePlaceholder = "#bytecode#";
        private const string UpdatedAtDatePlaceholder = "#updatedAt#";

        private readonly string directoryPath;
        private readonly int port;
        private readonly string contractsDirectoryPath;
        private readonly string testsDirectoryPath;
        private readonly string contractsBuildDirectory;

        private readonly string contractBuildTemplate = $@"
        {{
            ""contractName"": ""{ContractNamePlaceholder}"",
            ""abi"": {AbiPlaceholder},
            ""bytecode"": ""0x{ByteCodePlaceholder}"",
            ""networks"": {{}},
            ""updatedAt"": {UpdatedAtDatePlaceholder}
        }}";

        private readonly string migrationsContract = $@"
            pragma solidity ^0.4.23;

            contract {MigrationsContractName} {{
                address public owner;
                uint public last_completed_migration;

                constructor() public {{
                    owner = msg.sender;
                }}

                modifier restricted() {{
                    if (msg.sender == owner) _;
                }}

                function setCompleted(uint completed) public restricted {{
                    last_completed_migration = completed;
                }}

                function upgrade(address new_address) public restricted {{
                    {MigrationsContractName} upgraded = {MigrationsContractName}(new_address);
                    upgraded.setCompleted(last_completed_migration);
                }}
            }}";

        public TruffleProjectManager(string directoryPath, int port)
        {
            this.directoryPath = directoryPath;
            this.port = port;

            var (contractsDir, testsDir, contractsBuildDir) = this.CreateProjectStructure();

            this.contractsDirectoryPath = contractsDir;
            this.testsDirectoryPath = testsDir;
            this.contractsBuildDirectory = contractsBuildDir;
        }

        private string ConfigFile => $@"
            module.exports = {{
                networks: {{
                    development: {{
                        host: ""127.0.0.1"",
                        port: {this.port},
                        network_id: ""*"" // Match any network id
                    }}
                }}
            }};";

        public void InitializeMigration(string compilerPath)
        {
            var executor = new StandardProcessExecutor(0, 0);
            var migrationsContractPath = Path.Combine(this.contractsDirectoryPath, MigrationsFileName);

            var arguments = new[]
            {
                "--optimize",
                "--abi",
                "--bin",
                migrationsContractPath
            };

            var result = executor.Execute(
                compilerPath,
                string.Empty,
                DefaultTimeLimitInMilliseconds,
                DefaultMemoryLimitInBytes,
                arguments);

            if (!string.IsNullOrEmpty(result.ErrorOutput))
            {
                throw new Exception($"Compiling {MigrationsFileName} threw an exception: {result.ErrorOutput}");
            }

            var parts = result.ReceivedOutput.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 5)
            {
                return;
            }

            var byteCode = parts[2];
            var abi = parts[4];

            if (File.Exists(migrationsContractPath))
            {
                File.Delete(migrationsContractPath);
            }

            this.CreateJsonBuildForContracts(new Dictionary<string, (string byteCode, string abi)>
            {
                { MigrationsContractName, (byteCode, abi) }
            });
        }

        public void ImportJsUnitTests(IEnumerable<TestContext> tests)
        {
            var counter = 1;
            foreach (var test in tests)
            {
                var testName = $"{TestFileNamePrefix}{counter++}";
                File.WriteAllText(
                    Path.Combine(this.testsDirectoryPath, testName + JavaScriptFileExtension),
                    test.Input);
            }
        }

        public void CreateJsonBuildForContracts(Dictionary<string, (string byteCode, string abi)> compiledContracts)
        {
            var dateSettings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-ddTH:mm:ss.fffZ" };
            var date = JsonConvert.SerializeObject(DateTime.Now, dateSettings);

            foreach (var contract in compiledContracts)
            {
                var contractName = contract.Key;
                var byteCode = contract.Value.byteCode;
                var abi = contract.Value.abi;
                
                var buildContent = this.contractBuildTemplate
                    .Replace(ContractNamePlaceholder, contractName)
                    .Replace(AbiPlaceholder, abi)
                    .Replace(ByteCodePlaceholder, byteCode)
                    .Replace(UpdatedAtDatePlaceholder, date);

                File.WriteAllText(
                    Path.Combine(this.contractsBuildDirectory, contractName + JsonFileExtension),
                    buildContent);
            }
        }

        private (string contractsDir, string testsDir, string contractsBuildDir) CreateProjectStructure()
        {
            Directory.CreateDirectory(Path.Combine(this.directoryPath, MigrationsFolderName));
            var contractsDir = Directory.CreateDirectory(Path.Combine(this.directoryPath, ContractsFolderName));
            var testsDir = Directory.CreateDirectory(Path.Combine(this.directoryPath, TestsFolderName));
            var contractsBuildDir =
                Directory.CreateDirectory(Path.Combine(this.directoryPath, BuildsFolderName, ContractsFolderName));

            File.WriteAllText(
                Path.Combine(this.directoryPath, ConfigFileName + JavaScriptFileExtension),
                this.ConfigFile);

            File.WriteAllText(
                Path.Combine(contractsDir.FullName, MigrationsFileName),
                this.migrationsContract);

            return (contractsDir.FullName, testsDir.FullName, contractsBuildDir.FullName);
        }
    }
}