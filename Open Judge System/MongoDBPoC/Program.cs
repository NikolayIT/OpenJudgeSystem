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
            IMongoDatabase db = client.GetDatabase("OJS");
            var collection = db.GetCollection<SubmissionForProcessing>("submissionsForProcessing");

            InsertSubmissions(collection);

            var filter = new FilterDefinitionBuilder<SubmissionForProcessing>().Where(s => s.Processed);
            collection.Find(filter).ToList()
                .ForEach(s => Console.WriteLine(
                             $"ID: {s.SubmissionId}; Processed: {s.Processed}; Processing: {s.Processing}"));

        }

        private static void InsertSubmissions(IMongoCollection<SubmissionForProcessing> collection)
        {
            var submissionOne = new SubmissionForProcessing()
            {
                Processed = false,
                Processing = true,
                SubmissionId = 1
            };
            var submissionTwo = new SubmissionForProcessing()
            {
                Processed = false,
                Processing = false,
                SubmissionId = 2
            };
            var submissionThree = new SubmissionForProcessing()
            {
                Processed = true,
                Processing = false,
                SubmissionId = 3
            };

            var submissions = new List<SubmissionForProcessing>()
            {
                submissionOne,
                submissionTwo,
                submissionThree
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
