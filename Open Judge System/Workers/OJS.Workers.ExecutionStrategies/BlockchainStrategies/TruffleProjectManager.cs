namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using static OJS.Common.GlobalConstants;

    internal class TruffleProjectManager
    {
        private const string MigrationsContractName = "Migrations";
        private const string InitialMigrationFileName = "1_initial_migration";
        private const string ContractsDeployerFileName = "2_deploy_contracts";
        private const string ConfigFileName = "truffle-config";
        private const string TestsFolderName = "test";
        private const string MigrationsFolderName = "migrations";
        private const string ContractsFoldername = "contracts";
        private const string ContractNamePlaceholder = "#contractName#";
        private const string ContractsToImportTemplate = "#contractsToImport#";
        private const string ContractsToDeployTemplate = "#contractsToDeploy#";

        private readonly string directoryPath;
        private readonly int port;

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

            var(migrationsDir, contractsDir, testsDir) = this.CreateProjectStructure();

            this.MigrationsDirectoryPath = migrationsDir;
            this.ContractsDirectoryPath = contractsDir;
            this.TestsDirectoryPath = testsDir;
        }

        public string MigrationsDirectoryPath { get; }

        public string ContractsDirectoryPath { get; }

        public string TestsDirectoryPath { get; }

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

        public void CreateContract(string contractName, string contractContent)
        {
            File.WriteAllText(
                Path.Combine(this.ContractsDirectoryPath, contractName + SolidityFileExtension),
                contractContent);

            File.WriteAllText(
                Path.Combine(this.MigrationsDirectoryPath, ContractsDeployerFileName + JavaScriptFileExtension),
                this.GetDeployerForContracts(new[] { contractName }));
        }

        public void ImportTests()
        {
            
        }

        public void Test()
        {
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

        private(string migrationsDir, string contractsDir, string testsDir) CreateProjectStructure()
        {
            var migrationsDir = Directory.CreateDirectory(Path.Combine(this.directoryPath, MigrationsFolderName));
            var contractsDir = Directory.CreateDirectory(Path.Combine(this.directoryPath, ContractsFoldername));
            var testsDir = Directory.CreateDirectory(Path.Combine(this.directoryPath, TestsFolderName));

            File.WriteAllText(
                Path.Combine(this.directoryPath, ConfigFileName + JavaScriptFileExtension),
                this.ConfigFile);

            File.WriteAllText(
                Path.Combine(contractsDir.FullName, MigrationsContractName + SolidityFileExtension),
                this.migrationsContract);

            File.WriteAllText(
                Path.Combine(migrationsDir.FullName, InitialMigrationFileName + JavaScriptFileExtension),
                this.GetDeployerForContracts(new[] { MigrationsContractName }));

            return (migrationsDir.FullName, contractsDir.FullName, testsDir.FullName);
        }

        public void Compile()
        {
            throw new NotImplementedException();
        }
    }
}