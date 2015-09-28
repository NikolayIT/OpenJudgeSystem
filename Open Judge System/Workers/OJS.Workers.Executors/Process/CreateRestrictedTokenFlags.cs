namespace OJS.Workers.Executors.Process
{
    public enum CreateRestrictedTokenFlags
    {
        /// <summary>
        /// Disables all privileges in the new token except the SeChangeNotifyPrivilege privilege. If this value is specified, the DeletePrivilegeCount and PrivilegesToDelete parameters are ignored.
        /// </summary>
        DISABLE_MAX_PRIVILEGE = 0x1,

        /// <summary>
        /// If this value is used, the system does not check AppLocker rules or apply Software Restriction Policies. For AppLocker, this flag disables checks for all four rule collections: Executable, Windows Installer, Script, and DLL.
        /// When creating a setup program that must run extracted DLLs during installation, use the flag SAFER_TOKEN_MAKE_INERT in the SaferComputeTokenFromLevel function.
        /// A token can be queried for existence of this flag by using GetTokenInformation.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  On systems with KB2532445 installed, the caller must be running as LocalSystem or TrustedInstaller or the system ignores this flag. For more information, see "You can circumvent AppLocker rules by using an Office macro on a computer that is running Windows 7 or Windows Server 2008 R2" in the Help and Support Knowledge Base at http://support.microsoft.com/kb/2532445.
        /// Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  AppLocker is not supported. AppLocker was introduced in Windows 7 and Windows Server 2008 R2.
        /// </summary>
        SANDBOX_INERT = 0x2,

        /// <summary>
        /// The new token is a LUA token.
        /// Windows Server 2003 and Windows XP:  This value is not supported.
        /// </summary>
        LUA_TOKEN = 0x4,

        /// <summary>
        /// The new token contains restricting SIDs that are considered only when evaluating write access.
        /// Windows XP with SP2 and later:  The value of this constant is 0x4. For an application to be compatible with Windows XP with SP2 and later operating systems, the application should query the operating system by calling the GetVersionEx function to determine which value should be used.
        /// Windows Server 2003 and Windows XP with SP1 and earlier:  This value is not supported.
        /// </summary>
        WRITE_RESTRICTED = 0x8,
    }
}
