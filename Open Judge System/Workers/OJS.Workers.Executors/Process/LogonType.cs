namespace OJS.Workers.Executors.Process
{
    public enum LogonType
    {
        LOGON32_LOGON_INTERACTIVE = 2,
        LOGON32_LOGON_NETWORK,
        LOGON32_LOGON_BATCH,
        LOGON32_LOGON_SERVICE,
        LOGON32_LOGON_UNLOCK = 7,
        LOGON32_LOGON_NETWORK_CLEARTEXT,
        LOGON32_LOGON_NEW_CREDENTIALS
    }
}
