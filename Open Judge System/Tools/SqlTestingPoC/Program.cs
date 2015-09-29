namespace SqlTestingPoC
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlServerCe;
    using System.IO;

    public static class Program
    {
        public static void Main()
        {
            var databaseFileName = $"{Guid.NewGuid()}.sdf";
            var connectionString = $"DataSource=\"{databaseFileName}\"; Password=\"{Guid.NewGuid()}\"";
            using (var en = new SqlCeEngine(connectionString))
            {
                en.CreateDatabase();
            }

            using (var connection = new SqlCeConnection(connectionString))
            {
                connection.Open();

                var commands = new List<string>
                                   {
                                       @"CREATE TABLE [Cities]([Id] [int] NOT NULL, [Name] [ntext] NULL);",
                                       @"INSERT [Cities] ([Id], [Name]) VALUES (1, N'Sofia');",
                                       @"INSERT [Cities] ([Id], [Name]) VALUES (2, N'SofiaS');",
                                       @"INSERT [Cities] ([Id], [Name]) VALUES (3, N'NSofia');",
                                       @"INSERT [Cities] ([Id], [Name]) VALUES (4, N'NSofiaS');",
                                       @"INSERT [Cities] ([Id], [Name]) VALUES (5, N'Kyustendil');",
                                       @"INSERT [Cities] ([Id], [Name]) VALUES (6, N'S');"
                                   };
                foreach (var commandText in commands)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }

                // TODO: Time limit query
                var userCommand = connection.CreateCommand();
                userCommand.CommandText = "SELECT [Id], [Name] AS [Count] FROM [Cities] WHERE [Name] LIKE 'S%'";
                var userResult = new List<string>();
                using (var reader = userCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            // TODO: doubles? comma or dot?
                            userResult.Add(reader.GetValue(i).ToString());
                        }
                    }
                }

                foreach (var userItem in userResult)
                {
                    Console.WriteLine(userItem);
                }
            }

            File.Delete(databaseFileName);
        }
    }
}
