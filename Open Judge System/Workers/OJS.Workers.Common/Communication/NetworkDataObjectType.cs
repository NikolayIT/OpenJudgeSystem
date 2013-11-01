namespace OJS.Workers.Common.Communication
{
    public enum NetworkDataObjectType
    {
        AgentSendSystemInformation = 1, // c <-- a
        AgentSystemInformationReceived = 2, // c --> a
        AskAgentIfHasProblemDetails = 3, // c --> a
        AgentHasProblemDetailsInformation = 4, // c <-- a
        SendJobForAgent = 5, // c --> a
        JobDoneByAgent = 6, // c <-- a
        ShutdownAgent = 101, // c --> a
    }
}
