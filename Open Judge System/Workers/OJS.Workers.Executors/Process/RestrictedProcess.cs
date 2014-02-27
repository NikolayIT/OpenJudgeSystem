namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;

    using Microsoft.Win32.SafeHandles;
    using OJS.Workers.Executors.JobObjects;

    public class RestrictedProcess : IDisposable
    {
        private readonly SafeProcessHandle safeProcessHandle;
        private readonly string fileName = string.Empty;
        private ProcessInformation processInformation;
        private JobObject jobObject;
        private int exitCode;

        public RestrictedProcess(string fileName, string workingDirectory, IEnumerable<string> arguments = null, int bufferSize = 4096)
        {
            // Initialize fields
            this.fileName = fileName;
            this.IsDisposed = false;

            // Prepare startup info and redirect standard IO handles
            var startupInfo = new StartupInfo();
            this.RedirectStandardIoHandles(ref startupInfo, bufferSize);

            // Create restricted token
            var restrictedToken = this.CreateRestrictedToken();

            // Set mandatory label
            this.SetTokenMandatoryLabel(restrictedToken, SecurityMandatoryLabel.Low);

            var processSecurityAttributes = new SecurityAttributes();
            var threadSecurityAttributes = new SecurityAttributes();
            this.processInformation = new ProcessInformation();

            const uint CreationFlags = (uint)(
                CreateProcessFlags.CREATE_SUSPENDED |
                CreateProcessFlags.CREATE_BREAKAWAY_FROM_JOB |
                CreateProcessFlags.CREATE_UNICODE_ENVIRONMENT |
                CreateProcessFlags.CREATE_NEW_PROCESS_GROUP |
                CreateProcessFlags.DETACHED_PROCESS | // http://stackoverflow.com/questions/6371149/what-is-the-difference-between-detach-process-and-create-no-window-process-creat
                CreateProcessFlags.CREATE_NO_WINDOW) |
                (uint)ProcessPriorityClass.High;

            string commandLine;
            if (arguments != null)
            {
                var commandLineBuilder = new StringBuilder();
                commandLineBuilder.AppendFormat("\"{0}\"", fileName);
                foreach (var argument in arguments)
                {
                    commandLineBuilder.Append(' ');
                    commandLineBuilder.Append(argument);
                }

                commandLine = commandLineBuilder.ToString();
            }
            else
            {
                commandLine = fileName;
            }

            if (!NativeMethods.CreateProcessAsUser(
                    restrictedToken,
                    null,
                    commandLine,
                    processSecurityAttributes,
                    threadSecurityAttributes,
                    true, // In order to standard input, output and error redirection work, the handles must be inheritable and the CreateProcess() API must specify that inheritable handles are to be inherited by the child process by specifying TRUE in the bInheritHandles parameter. 
                    CreationFlags,
                    IntPtr.Zero,
                    workingDirectory,
                    startupInfo,
                    out this.processInformation))
            {
                throw new Win32Exception();
            }

            this.safeProcessHandle = new SafeProcessHandle(this.processInformation.Process);
            
            // This is a very important line! Without disposing the startupInfo handles, reading the standard output (or error) will hang forever.
            // Same problem described here: http://social.msdn.microsoft.com/Forums/vstudio/en-US/3c25a2e8-b1ea-4fc4-927b-cb865d435147/how-does-processstart-work-in-getting-output
            startupInfo.Dispose();

            NativeMethods.CloseHandle(restrictedToken);
        }

        public StreamWriter StandardInput { get; private set; }

        public StreamReader StandardOutput { get; private set; }

        public StreamReader StandardError { get; private set; }

        public int Id
        {
            get
            {
                return this.processInformation.ProcessId;
            }
        }

        public int MainThreadId
        {
            get
            {
                return this.processInformation.ThreadId;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return this.processInformation.Process;
            }
        }

        public IntPtr MainThreadHandle
        {
            get
            {
                return this.processInformation.Thread;
            }
        }

        public bool HasExited
        {
            get
            {
                if (this.safeProcessHandle.IsInvalid || this.safeProcessHandle.IsClosed)
                {
                    return true;
                }

                if (NativeMethods.GetExitCodeProcess(this.safeProcessHandle, out this.exitCode) && this.exitCode != NativeMethods.STILL_ACTIVE)
                {
                    return true;
                }
                
                return false;
            }
        }

        public int ExitCode
        {
            get
            {
                if (!this.HasExited)
                {
                    throw new Exception("Process is still active!");
                }

                return this.exitCode;
            }
        }

        public string ExitCodeAsString
        {
            get
            {
                return new Win32Exception(this.ExitCode).Message;
            }
        }

        /// <summary>
        /// Returns the time the process was started.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return this.GetProcessTimes().StartTime;
            }
        }

        /// <summary>
        /// Gets the time that the process exited.
        /// </summary>
        public DateTime ExitTime
        {
            get
            {
                return this.GetProcessTimes().ExitTime;
            }
        }

        /// <summary>
        /// Returns the amount of time the process has spent running code inside the operating system core.
        /// </summary>
        public TimeSpan PrivilegedProcessorTime
        {
            get
            {
                return this.GetProcessTimes().PrivilegedProcessorTime;
            }
        }

        /// <summary>
        /// Returns the amount of time the associated process has spent running code inside the application portion of the process (not the operating system core).
        /// </summary>
        public TimeSpan UserProcessorTime
        {
            get
            {
                return this.GetProcessTimes().UserProcessorTime;
            }
        }

        /// <summary>
        /// Returns the amount of time the associated process has spent utilizing the CPU.
        /// </summary>
        public TimeSpan TotalProcessorTime
        {
            get
            {
                return this.GetProcessTimes().TotalProcessorTime;
            }
        }

        /// <summary>
        /// Warning: If two processes with the same name are created, this property may not return correct name!
        /// </summary>
        public string Name
        {
            get
            {
                var fileNameOnly = new FileInfo(this.fileName).Name;
                if (this.fileName.EndsWith(".exe"))
                {
                    return fileNameOnly.Substring(0, fileNameOnly.Length - 4);
                }

                return fileNameOnly;
            }
        }

        public long PeakWorkingSetSize
        {
            get
            {
                var counters = new ProcessMemoryCounters();
                NativeMethods.GetProcessMemoryInfo(this.Handle, out counters, (uint)Marshal.SizeOf(counters));
                return (int)counters.PeakWorkingSetSize;
            }
        }

        public long PeakPagefileUsage
        {
            get
            {
                var counters = new ProcessMemoryCounters();
                NativeMethods.GetProcessMemoryInfo(this.Handle, out counters, (uint)Marshal.SizeOf(counters));
                return (int)counters.PeakPagefileUsage;
            }
        }

        public bool IsDisposed { get; private set; }

        public void Start(int timeLimit, int memoryLimit)
        {
            try
            {
                this.jobObject = new JobObject();
                this.jobObject.SetExtendedLimitInformation(PrepareJobObject.GetExtendedLimitInformation(timeLimit * 2, memoryLimit * 2));
                this.jobObject.SetBasicUiRestrictions(PrepareJobObject.GetUiRestrictions());
                this.jobObject.AddProcess(this.processInformation.Process);

                NativeMethods.ResumeThread(this.processInformation.Thread);
            }
            catch (Win32Exception)
            {
                this.Kill();
                throw;
            }
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void Kill()
        {
            NativeMethods.TerminateProcess(this.safeProcessHandle, -1);
        }

        public bool WaitForExit(int milliseconds)
        {
            var result = NativeMethods.WaitForSingleObject(this.processInformation.Process, (uint)milliseconds);
            return result != 258; // TODO: Extract as constant and check all cases (http://msdn.microsoft.com/en-us/library/windows/desktop/ms687032%28v=vs.85%29.aspx)
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.IsDisposed = true;
                this.safeProcessHandle.Dispose();
                NativeMethods.CloseHandle(this.processInformation.Thread);
                this.jobObject.Dispose();

                // Disposing these object causes "System.InvalidOperationException: The stream is currently in use by a previous operation on the stream."
                // this.StandardInput.Dispose();
                // this.StandardOutput.Dispose();
                // this.StandardError.Dispose();
            }
        }

        private void RedirectStandardIoHandles(ref StartupInfo startupInfo, int bufferSize)
        {
            // Some of this code is based on System.Diagnostics.Process.StartWithCreateProcess method implementation
            SafeFileHandle standardInputWritePipeHandle;
            SafeFileHandle standardOutputReadPipeHandle;
            SafeFileHandle standardErrorReadPipeHandle;

            // http://support.microsoft.com/kb/190351 (How to spawn console processes with redirected standard handles)
            // If the dwFlags member is set to STARTF_USESTDHANDLES, then the following STARTUPINFO members specify the standard handles of the child console based process: 
            // HANDLE hStdInput - Standard input handle of the child process.
            // HANDLE hStdOutput - Standard output handle of the child process.
            // HANDLE hStdError - Standard error handle of the child process.
            startupInfo.Flags = (int)StartupInfoFlags.STARTF_USESTDHANDLES;
            this.CreatePipe(out standardInputWritePipeHandle, out startupInfo.StandardInputHandle, true, bufferSize);
            this.CreatePipe(out standardOutputReadPipeHandle, out startupInfo.StandardOutputHandle, false, bufferSize);
            this.CreatePipe(out standardErrorReadPipeHandle, out startupInfo.StandardErrorHandle, false, 4096);

            this.StandardInput = new StreamWriter(new FileStream(standardInputWritePipeHandle, FileAccess.Write, bufferSize, false), Encoding.Default, bufferSize)
                                     {
                                         AutoFlush = true
                                     };
            this.StandardOutput = new StreamReader(new FileStream(standardOutputReadPipeHandle, FileAccess.Read, bufferSize, false), Encoding.Default, true, bufferSize);
            this.StandardError = new StreamReader(new FileStream(standardErrorReadPipeHandle, FileAccess.Read, 4096, false), Encoding.Default, true, 4096);
            
            /*
             * Child processes that use such C run-time functions as printf() and fprintf() can behave poorly when redirected.
             * The C run-time functions maintain separate IO buffers. When redirected, these buffers might not be flushed immediately after each IO call.
             * As a result, the output to the redirection pipe of a printf() call or the input from a getch() call is not flushed immediately and delays, sometimes-infinite delays occur.
             * This problem is avoided if the child process flushes the IO buffers after each call to a C run-time IO function.
             * Only the child process can flush its C run-time IO buffers. A process can flush its C run-time IO buffers by calling the fflush() function.
             */
        }

        private void CreatePipeWithSecurityAttributes(out SafeFileHandle readPipe, out SafeFileHandle writePipe, SecurityAttributes pipeAttributes, int size)
        {
            if (!NativeMethods.CreatePipe(out readPipe, out writePipe, pipeAttributes, size) || readPipe.IsInvalid
                || writePipe.IsInvalid)
            {
                throw new Win32Exception();
            }
        }

        // Using synchronous Anonymous pipes for process input/output redirection means we would end up 
        // wasting a worker thread pool thread per pipe instance. Overlapped pipe IO is desirable, since
        // it will take advantage of the NT IO completion port infrastructure. But we can't really use 
        // Overlapped I/O for process input/output as it would break Console apps (managed Console class
        // methods such as WriteLine as well as native CRT functions like printf) which are making an
        // assumption that the console standard handles (obtained via GetStdHandle()) are opened
        // for synchronous I/O and hence they can work fine with ReadFile/WriteFile synchronously! 
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void CreatePipe(out SafeFileHandle parentHandle, out SafeFileHandle childHandle, bool parentInputs, int bufferSize)
        {
            var securityAttributesParent = new SecurityAttributes { InheritHandle = true };

            SafeFileHandle tempHandle = null;
            try
            {
                if (parentInputs)
                {
                    this.CreatePipeWithSecurityAttributes(out childHandle, out tempHandle, securityAttributesParent, bufferSize);
                }
                else
                {
                    this.CreatePipeWithSecurityAttributes(out tempHandle, out childHandle, securityAttributesParent, bufferSize);
                }

                // Duplicate the parent handle to be non-inheritable so that the child process 
                // doesn't have access. This is done for correctness sake, exact reason is unclear.
                // One potential theory is that child process can do something brain dead like 
                // closing the parent end of the pipe and there by getting into a blocking situation 
                // as parent will not be draining the pipe at the other end anymore.

                // Create a duplicate of the output write handle for the std error write handle.
                // This is necessary in case the child application closes one of its std output handles.
                if (!NativeMethods.DuplicateHandle(
                        new HandleRef(this, NativeMethods.GetCurrentProcess()),
                        tempHandle,
                        new HandleRef(this, NativeMethods.GetCurrentProcess()),
                        out parentHandle, // Address of new handle.
                        0,
                        false, // Make it un-inheritable.
                        (int)DuplicateOptions.DUPLICATE_SAME_ACCESS))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                // Close inheritable copies of the handles you do not want to be inherited.
                if (tempHandle != null && !tempHandle.IsInvalid)
                {
                    tempHandle.Close();
                }
            }
        }

        private IntPtr CreateRestrictedToken()
        {
            // Open the current process and grab its primary token
            IntPtr processToken;
            if (!NativeMethods.OpenProcessToken(
                    NativeMethods.GetCurrentProcess(),
                    NativeMethods.TOKEN_DUPLICATE | NativeMethods.TOKEN_ASSIGN_PRIMARY | NativeMethods.TOKEN_QUERY | NativeMethods.TOKEN_ADJUST_DEFAULT,
                    out processToken))
            {
                throw new Win32Exception();
            }

            // Create the restricted token for the process
            IntPtr restrictedToken;
            if (!NativeMethods.CreateRestrictedToken(
                    processToken,
                    CreateRestrictedTokenFlags.SANDBOX_INERT, // TODO: DISABLE_MAX_PRIVILEGE ??
                    //// Disable SID
                    0,
                    null,
                    //// Delete privilege
                    0,
                    null,
                    //// Restricted SID
                    0,
                    null,
                    out restrictedToken))
            {
                throw new Win32Exception();
            }

            // Clean up our mess
            NativeMethods.CloseHandle(processToken);

            return restrictedToken;
        }

        private IntPtr CreateRestrictedTokenWithSafer()
        {
            IntPtr saferLevel;
            IntPtr token;

            if (!NativeMethods.SaferCreateLevel(
                    NativeMethods.SAFER_SCOPEID_USER,
                    NativeMethods.SAFER_LEVELID_CONSTRAINED,
                    NativeMethods.SAFER_LEVEL_OPEN,
                    out saferLevel,
                    IntPtr.Zero))
            {
                throw new Win32Exception();
            }

            if (!NativeMethods.SaferComputeTokenFromLevel(saferLevel, IntPtr.Zero, out token, 0, IntPtr.Zero))
            {
                NativeMethods.SaferCloseLevel(saferLevel);
                throw new Win32Exception();
            }

            NativeMethods.SaferCloseLevel(saferLevel);

            return token;
        }
        
        private void SetTokenMandatoryLabel(IntPtr token, SecurityMandatoryLabel securityMandatoryLabel)
        {
            // Create the low integrity SID.
            IntPtr integritySid;
            if (!NativeMethods.AllocateAndInitializeSid(
                    ref NativeMethods.SECURITY_MANDATORY_LABEL_AUTHORITY,
                    1,
                    (int)securityMandatoryLabel,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    out integritySid))
            {
                throw new Win32Exception();
            }

            var tokenMandatoryLabel = new TokenMandatoryLabel { Label = new SidAndAttributes() };
            tokenMandatoryLabel.Label.Attributes = NativeMethods.SE_GROUP_INTEGRITY;
            tokenMandatoryLabel.Label.Sid = integritySid;
            //// Marshal the TOKEN_MANDATORY_LABEL structure to the native memory.
            var sizeOfTokenMandatoryLabel = Marshal.SizeOf(tokenMandatoryLabel);
            var tokenInfo = Marshal.AllocHGlobal(sizeOfTokenMandatoryLabel);
            Marshal.StructureToPtr(tokenMandatoryLabel, tokenInfo, false);

            // Set the integrity level in the access token
            if (!NativeMethods.SetTokenInformation(
                    token,
                    TokenInformationClass.TokenIntegrityLevel,
                    tokenInfo,
                    sizeOfTokenMandatoryLabel + NativeMethods.GetLengthSid(integritySid)))
            {
                throw new Win32Exception();
            }

            //// SafeNativeMethods.CloseHandle(integritySid);
            //// SafeNativeMethods.CloseHandle(tokenInfo);
        }

        private ProcessThreadTimes GetProcessTimes()
        {
            var processTimes = new ProcessThreadTimes();
            if (!NativeMethods.GetProcessTimes(
                    this.safeProcessHandle,
                    out processTimes.Create,
                    out processTimes.Exit,
                    out processTimes.Kernel,
                    out processTimes.User))
            {
                throw new Win32Exception();
            }

            return processTimes;
        }
    }
}
