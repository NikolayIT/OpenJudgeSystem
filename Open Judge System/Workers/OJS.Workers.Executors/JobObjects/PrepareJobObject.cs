namespace OJS.Workers.Executors.JobObjects
{
    using System;

    internal static class PrepareJobObject
    {
        public static ExtendedLimitInformation GetExtendedLimitInformation(int maximumTime, int maximumMemory)
        {
            var info = new BasicLimitInformation
            {
                LimitFlags =
                    (int)(LimitFlags.JOB_OBJECT_LIMIT_JOB_MEMORY
                     //// The following two flags are causing the process to have unexpected behavior
                     //// | LimitFlags.JOB_OBJECT_LIMIT_JOB_TIME
                     //// | LimitFlags.JOB_OBJECT_LIMIT_PROCESS_TIME
                     | LimitFlags.JOB_OBJECT_LIMIT_ACTIVE_PROCESS
                     | LimitFlags.JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION
                     | LimitFlags.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE),
                PerJobUserTimeLimit = maximumTime, // TODO: Remove or rework
                PerProcessUserTimeLimit = maximumTime,
                ActiveProcessLimit = 1,
            };

            var extendedInfo = new ExtendedLimitInformation
            {
                BasicLimitInformation = info,
                JobMemoryLimit = (UIntPtr)maximumMemory,
                IoInfo =
                {
                    ReadTransferCount = 0,
                    ReadOperationCount = 0,
                    WriteOperationCount = 0,
                    WriteTransferCount = 0
                }
            };

            return extendedInfo;
        }

        public static BasicUiRestrictions GetUiRestrictions()
        {
            var restrictions = new BasicUiRestrictions
                                   {
                                       UIRestrictionsClass =
                                           (int)(UiRestrictionFlags.JOB_OBJECT_UILIMIT_DESKTOP
                                            | UiRestrictionFlags.JOB_OBJECT_UILIMIT_DISPLAYSETTINGS
                                            | UiRestrictionFlags.JOB_OBJECT_UILIMIT_EXITWINDOWS
                                            | UiRestrictionFlags.JOB_OBJECT_UILIMIT_GLOBALATOMS
                                            | UiRestrictionFlags.JOB_OBJECT_UILIMIT_HANDLES
                                            | UiRestrictionFlags.JOB_OBJECT_UILIMIT_READCLIPBOARD
                                            | UiRestrictionFlags.JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS
                                            | UiRestrictionFlags.JOB_OBJECT_UILIMIT_WRITECLIPBOARD)
                                   };

            return restrictions;
        }
    }
}
