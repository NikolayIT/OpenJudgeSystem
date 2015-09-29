namespace SqlTestingPoC
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlServerCe;
    using System.IO;

    using MissingFeatures;

    public static class Program
    {
        public static void Main()
        {
            var databaseFileName = $"{Guid.NewGuid()}.sdf";
            var connectionString = $"DataSource=\"{databaseFileName}\"; Password=\"{Guid.NewGuid()}\";";
            using (var en = new SqlCeEngine(connectionString))
            {
                en.CreateDatabase();
            }

            using (var connection = new SqlCeConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand().MultiQuery();
                command.CommandText = @"
CREATE TABLE [Cities]([Id] [int] NOT NULL, [Name] [ntext] NULL);
INSERT [Cities] ([Id], [Name]) VALUES (1, N'Sofia');
INSERT [Cities] ([Id], [Name]) VALUES (2, N'SofiaS');
INSERT [Cities] ([Id], [Name]) VALUES (3, N'NSo;fia');
INSERT [Cities] ([Id], [Name]) VALUES (4, N'NSofiaS');
INSERT [Cities] ([Id], [Name]) VALUES (5, N'Kyustendil');
INSERT [Cities] ([Id], [Name]) VALUES (6, N'S');";
                command.ExecuteNonQuery();

                var userCommand = connection.CreateCommand().MultiQuery();

                userCommand.CommandText = @"SELECT * FROM [Cities]
CROSS JOIN [Cities] c1
CROSS JOIN [Cities] c2
CROSS JOIN [Cities] c3
CROSS JOIN [Cities] c4
CROSS JOIN [Cities] c5
CROSS JOIN [Cities] c6
CROSS JOIN [Cities] c7
CROSS JOIN [Cities] c8
CROSS JOIN [Cities] c9
CROSS JOIN [Cities] c10
CROSS JOIN [Cities] c11";
                var userResult = new List<string>();

                // TODO: Abort on timeout
                var completed = Code.ExecuteWithTimeLimit(
                    TimeSpan.FromMilliseconds(1000),
                    () =>
                        {
                            var reader = userCommand.ExecuteReader();
                            using (reader)
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
                        });

                if (completed)
                {
                    foreach (var userItem in userResult)
                    {
                        Console.WriteLine(userItem);
                    }
                }
                else
                {
                    Console.WriteLine("Timeout!");
                }
            }

            File.Delete(databaseFileName);
        }
    }
}
