namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    using Ionic.Zip;

    public class RemoteExecutionStrategy : IExecutionStrategy
    {
        public ExecutionResult Execute(ExecutionContext executionContext)
        {
            throw new NotImplementedException();

            /*
            var request = new PayloadSettings
                              {
                                  Id = executionContext.SubmissionId,
                                  TimeLimit = executionContext.TimeLimit,
                                  MemoryLimit = executionContext.MemoryLimit,
                                  Payload = this.PreparePayloadFile(executionContext)
                              };

            var client = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Post, "http://79.124.67.13:9000/execute");

            client.SendAsync(message).ContinueWith(
                (response =>
                    {
                        var serializer = new JsonSerializer();
                        serializer.Deserialize<ServerResponse>(
                            new JsonReader { FloatParseHandling = FloatParseHandling.Decimal });
                        // TODO: Deserialize response
                    }));

            // TODO: Parse response
             */
        }

        private byte[] PreparePayloadFile(ExecutionContext executionContext)
        {
            var tempFile = Path.GetTempFileName();
            tempFile += ".zip"; // TODO: Useless?

            var zip = new ZipFile(tempFile);

            zip.AddEntry("userCode.deflate", executionContext.FileContent);

            zip.AddDirectory("tests");
            foreach (var test in executionContext.Tests)
            {
                zip.AddEntry(string.Format("/tests/{0}", test.Id), test.Input);
            }

            // TODO: Optimize?
            var stream = new MemoryStream();
            zip.Save(stream);
            stream.Position = 0;
            var byteArray = stream.ToArray();

            return byteArray;
        }

        // TODO: Document
        private class PayloadSettings
        {
            public int Id { get; set; }

            public int TimeLimit { get; set; }

            public int MemoryLimit { get; set; }

            public byte[] Payload { get; set; }
        }
    }
}
