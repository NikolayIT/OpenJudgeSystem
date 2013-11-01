namespace OJS.Workers.Executors.Process
{
    public enum SecurityMandatoryLabel
    {
        /// <summary>
        /// Processes that are logged on anonymously are automatically designated as Untrusted
        /// SID: S-1-16-0x0
        /// </summary>
        Untrusted = 0x00000000,

        /// <summary>
        /// The Low integrity level is the level used by default for interaction with the Internet.
        /// As long as Internet Explorer is run in its default state, Protected Mode, all files and processes associated with it are assigned the Low integrity level.
        /// Some folders, such as the Temporary Internet Folder, are also assigned the Low integrity level by default.
        /// SID: S-1-16-0x1000
        /// </summary>
        Low = 0x00001000,

        /// <summary>
        /// Medium is the context that most objects will run in.
        /// Standard users receive the Medium integrity level, and any object not explicitly designated with a lower or higher integrity level is Medium by default.
        /// SID: S-1-16-0x2000
        /// </summary>
        Medium = 0x00002000,

        /// <summary>
        /// Administrators are granted the High integrity level.
        /// This ensures that Administrators are capable of interacting with and modifying objects assigned Medium or Low integrity levels, but can also act on other objects with a High integrity level, which standard users can not do.
        /// SID: S-1-16-0x3000
        /// </summary>
        High = 0x00003000,

        /// <summary>
        /// As the name implies, the System integrity level is reserved for the system.
        /// The Windows kernel and core services are granted the System integrity level.
        /// Being even higher than the High integrity level of Administrators protects these core functions from being affected or compromised even by Administrators.
        /// SID: S-1-16-0x4000
        /// </summary>
        System = 0x00004000,
    }
}
