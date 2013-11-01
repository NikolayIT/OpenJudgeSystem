namespace OJS.Workers.Executors.JobObjects
{
    public enum InfoClass
    {
        BasicAccountingInformation = 1,
        BasicLimitInformation = 2,
        BasicProcessIdList = 3,
        BasicUiRestrictions = 4,
        SecurityLimitInformation = 5,  // deprecated
        EndOfJobTimeInformation = 6,
        AssociateCompletionPortInformation = 7,
        BasicAndIoAccountingInformation = 8,
        ExtendedLimitInformation = 9,
        JobSetInformation = 10,
        GroupInformation = 11,
        NotificationLimitInformation = 12,
        LimitViolationInformation = 13,
        GroupInformationEx = 14,
        CpuRateControlInformation = 15,
        CompletionFilter = 16,
        CompletionCounter = 17,
        Reserved1Information = 18,
        Reserved2Information = 19,
        Reserved3Information = 20,
        Reserved4Information = 21,
        Reserved5Information = 22,
        Reserved6Information = 23,
        Reserved7Information = 24,
        Reserved8Information = 25,
        Reserved9Information = 26,
        MaxJobObjectInfoClass = 27,
    }
}
