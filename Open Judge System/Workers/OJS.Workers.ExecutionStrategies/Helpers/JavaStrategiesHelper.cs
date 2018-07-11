namespace OJS.Workers.ExecutionStrategies.Helpers
{
    using System;

    internal static class JavaStrategiesHelper
    {
        private const string JvmInsufficientMemoryMessage =
            "There is insufficient memory for the Java Runtime Environment to continue.";

        private const string JvmFailedToReserveMemoryMessage =
            "Failed to allocate initial concurrent mark overflow mark stack.";

        private const string JvmInitializationErrorMessage = "Error occurred during initialization of VM";

        /// <summary>
        /// Validates if the Java Virtual Machine has been initialized successfully,
        /// by checking for critical error messages and throws exception when has any.
        /// </summary>
        /// <param name="processRecievedOutput">The recieved output from the process executor</param>
        public static void ValidateJvmInitialization(string processRecievedOutput)
        {
            const string errorMessageAppender = " Please contact an administrator.";

            if (processRecievedOutput.Contains(JvmInsufficientMemoryMessage))
            {
                throw new InsufficientMemoryException(JvmInsufficientMemoryMessage + errorMessageAppender);
            }

            if (processRecievedOutput.Contains(JvmFailedToReserveMemoryMessage))
            {
                throw new InsufficientMemoryException(JvmFailedToReserveMemoryMessage + errorMessageAppender);
            }

            if (processRecievedOutput.Contains(JvmInitializationErrorMessage))
            {
                throw new Exception(JvmInitializationErrorMessage + errorMessageAppender);
            }
        }
    }
}