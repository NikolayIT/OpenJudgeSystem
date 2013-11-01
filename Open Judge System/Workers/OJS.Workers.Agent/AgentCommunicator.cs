namespace OJS.Workers.Agent
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    using log4net;

    using OJS.Workers.Common.Communication;

    // TODO: Extract communication logic into new class called AgentCommunicator
    public class AgentCommunicator
    {
        private readonly ILog logger;
        private readonly IFormatter formatter;
        private readonly IJobWorker jobWorker;
        private readonly IFileCache fileCache;
        private readonly Stream stream;

        public AgentCommunicator(Stream stream, IFormatter formatter, IJobWorker jobWorker, IFileCache fileCache, ILog logger)
        {
            logger.Info("AgentCommunicator initializing...");

            this.stream = stream;
            this.formatter = formatter;
            this.jobWorker = jobWorker;
            this.fileCache = fileCache;
            this.logger = logger;

            logger.Info("AgentCommunicator initialized.");
        }

        /// <summary>
        /// Start the agent client and wait for tasks from controller.
        /// This operation is blocking.
        /// </summary>
        internal void StartCommunicationLoop()
        {
            this.SendSystemInformation();

            this.ReceiveHello();

            // Wait for tasks
            while (true)
            {
                this.NegotiateForTaskExistence();
                var task = this.GetJob();
                var jobResult = this.jobWorker.DoJob(task);
                this.SendJobResult(jobResult);
            }
        }

        private void SendJobResult(JobResult jobResult)
        {
            var dataObject = new NetworkDataObject
            {
                Data = jobResult,
                Type = NetworkDataObjectType.JobDoneByAgent,
            };

            this.formatter.Serialize(this.stream, dataObject);
        }

        private void NegotiateForTaskExistence()
        {
            var result = this.formatter.Deserialize(this.stream) as NetworkDataObject;
            if (result == null || result.Type != NetworkDataObjectType.AskAgentIfHasProblemDetails)
            {
                throw new NetworkCommunicationException();
            }

            var taskKey = result.Data as string;
            var taskExists = this.fileCache.Contains(taskKey);

            var dataObject = new NetworkDataObject
            {
                Data = taskExists,
                Type = NetworkDataObjectType.AgentHasProblemDetailsInformation,
            };

            this.formatter.Serialize(this.stream, dataObject);
        }

        private Job GetJob()
        {
            var result = this.formatter.Deserialize(this.stream) as NetworkDataObject;
            if (result == null || result.Type != NetworkDataObjectType.SendJobForAgent)
            {
                throw new NetworkCommunicationException();
            }

            var task = result.Data as Job;

            return task;
        }

        private void SendSystemInformation()
        {
            try
            {
                var dataObject = new NetworkDataObject
                                     {
                                         Data = SystemInformation.Collect(),
                                         Type = NetworkDataObjectType.AgentSendSystemInformation
                                     };

                this.formatter.Serialize(this.stream, dataObject);
            }
            catch (Exception exception)
            {
                this.logger.FatalFormat(
                    "Unable to send system information to the controller. Caught Exception: {0}",
                    exception);
                throw;
            }
        }

        private void ReceiveHello()
        {
            try
            {
                var result = this.formatter.Deserialize(this.stream) as NetworkDataObject;

                //// TODO: Do something with controller information in result.Data

                if (result == null || result.Type != NetworkDataObjectType.AgentSystemInformationReceived)
                {
                    throw new NetworkCommunicationException();
                }
            }
            catch (Exception exception)
            {
                this.logger.FatalFormat(
                    "Unable to receive system information from the controller. Caught Exception: {0}",
                    exception);
                throw;
            }
        }
    }
}
