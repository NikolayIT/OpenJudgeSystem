namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using OJS.Workers.Executors;

    using static OJS.Common.GlobalConstants;

    internal class TruffleProjectManager
    {
        private const string MigrationsContractName = "Migrations";
        private const string MigrationsFileName = MigrationsContractName + SolidityFileExtension;
        private const string InitialMigrationFileName = "1_initial_migration";
        private const string ContractsDeployerFileName = "2_deploy_contracts";
        private const string ConfigFileName = "truffle-config";
        private const string TestsFolderName = "test";
        private const string MigrationsFolderName = "migrations";
        private const string ContractsFoldername = "contracts";
        private const string BuildsFolderName = "build";
        private const string ContractNamePlaceholder = "#contractName#";
        private const string ContractsToImportTemplate = "#contractsToImport#";
        private const string ContractsToDeployTemplate = "#contractsToDeploy#";
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

        private readonly string deployerTemplete = $@"
            {ContractsToImportTemplate}
            module.exports = function (deployer) {{
                {ContractsToDeployTemplate}
            }};";

        private readonly string contractImportTemplate =
            $@"const {ContractNamePlaceholder} = artifacts.require('./{ContractNamePlaceholder}{SolidityFileExtension}');";

        private readonly string contractDeployTemplate = $@"deployer.deploy({ContractNamePlaceholder});";

        public TruffleProjectManager(string directoryPath, int port)
        {
            this.directoryPath = directoryPath;
            this.port = port;

            var(contractsDir, testsDir, contractsBuildDir) = this.CreateProjectStructure();

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
                ProblemDefaultTimeLimit,
                ProblemDefaultMemoryLimit,
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

            this.CreateBuildForContract(MigrationsContractName, abi, byteCode);
        }

        public void ImportJsUnitTests(IEnumerable<TestContext> tests)
        {
            var counter = 1;
            foreach (var test in tests)
            {
                var testName = $"Test{counter++}";
                File.WriteAllText(
                    Path.Combine(this.testsDirectoryPath, testName + JavaScriptFileExtension),
                    test.Input);
            }
        }

        public void CreateBuildForContract(
            string contractName,
            string abi,
            string bytecode)
        {
            var dateSettings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-ddTH:mm:ss.fffZ" };
            var date = JsonConvert.SerializeObject(DateTime.Now, dateSettings);

            var buildContent = this.contractBuildTemplate
                .Replace(ContractNamePlaceholder, contractName)
                .Replace(AbiPlaceholder, abi)
                .Replace(ByteCodePlaceholder, bytecode)
                .Replace(UpdatedAtDatePlaceholder, date);

            File.WriteAllText(
                Path.Combine(this.contractsBuildDirectory, contractName + JsonFileExtension),
                buildContent);
        }

        private string GetDeployerForContracts(IEnumerable<string> contractNames)
        {
            var imports = new List<string>();
            var deploys = new List<string>();

            foreach (var contractName in contractNames)
            {
                imports.Add(this.contractImportTemplate.Replace(ContractNamePlaceholder, contractName));
                deploys.Add(this.contractDeployTemplate.Replace(ContractNamePlaceholder, contractName));
            }

            return this.deployerTemplete
                .Replace(ContractsToImportTemplate, string.Join(Environment.NewLine, imports))
                .Replace(ContractsToDeployTemplate, string.Join(Environment.NewLine, deploys));
        }

        private(string contractsDir, string testsDir, string contractsBuildDir) CreateProjectStructure()
        {
            Directory.CreateDirectory(Path.Combine(this.directoryPath, MigrationsFolderName));
            var contractsDir = Directory.CreateDirectory(Path.Combine(this.directoryPath, ContractsFoldername));
            var testsDir = Directory.CreateDirectory(Path.Combine(this.directoryPath, TestsFolderName));
            var contractsBuildDir =
                Directory.CreateDirectory(Path.Combine(this.directoryPath, BuildsFolderName, ContractsFoldername));

            File.WriteAllText(
                Path.Combine(this.directoryPath, ConfigFileName + JavaScriptFileExtension),
                this.ConfigFile);

            File.WriteAllText(
                Path.Combine(contractsDir.FullName, MigrationsFileName),
                this.migrationsContract);

            //File.WriteAllText(
            //    Path.Combine(migrationsDir.FullName, InitialMigrationFileName + JavaScriptFileExtension),
            //    this.GetDeployerForContracts(new[] { MigrationsContractName }));

            return (contractsDir.FullName, testsDir.FullName, contractsBuildDir.FullName);
        }
    }
}