namespace MongoDBPoC
{
    using System;
    using System.Collections.Generic;

    using MongoDB.Bson;
    using MongoDB.Driver;

    public class Program
    {
        static void Main()
        {
            const string ConnectionString = "mongodb://localhost:27017";
            var client = new MongoClient(ConnectionString);
            var db = client.GetDatabase("OJS");
            var submissionsForProcessingCollection = db
                .GetCollection<SubmissionForProcessing>("submissionsForProcessing");

            InsertSubmissions(submissionsForProcessingCollection);

            var processedSubmissionsFilter = 
                new FilterDefinitionBuilder<SubmissionForProcessing>()
                .Where(s => s.Processed);

            submissionsForProcessingCollection
                .Find(processedSubmissionsFilter)
                .ToList()
                .ForEach(s => Console.WriteLine(
                    $"ID: {s.SubmissionId}; Processed: {s.Processed}; Processing: {s.Processing}"));
        }

        private static void InsertSubmissions(IMongoCollection<SubmissionForProcessing> collection)
        {
            var submissions = new List<SubmissionForProcessing>()
            {
               new SubmissionForProcessing
               {
                    Processed = false,
                    Processing = true,
                    SubmissionId = 1
                },
                new SubmissionForProcessing
                {
                    Processed = false,
                    Processing = false,
                    SubmissionId = 2
                },
                new SubmissionForProcessing
                {
                Processed = true,
                Processing = false,
                SubmissionId = 3
                }
            };

            collection.InsertMany(submissions);
        }

        private class SubmissionForProcessing
        {
            public ObjectId Id { get; set; }

            public int SubmissionId { get; set; }

            public bool Processing { get; set; }

            public bool Processed { get; set; }
        }
    }
}
