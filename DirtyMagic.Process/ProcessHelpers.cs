using DirtyMagic.Exceptions;
using DirtyMagic.Processes;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Structures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DirtyMagic
{
    public static class ProcessHelpers
    {
        /// <summary>
        /// Use this as alternative to builtin Process.GetProcesses() in
        /// order not to deal with exceptions
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<RemoteProcess> EnumerateProcesses()
        {
            const uint arraySize = 1024u;
            var arrayBytesSize = arraySize * sizeof(uint);
            var processIds = new int[arraySize];

            if (!Psapi.EnumProcesses(processIds, arrayBytesSize, out var bytesCopied))
                yield break;

            if (bytesCopied == 0)
                yield break;

            if ((bytesCopied & 3) != 0)
                yield break;

            var numIdsCopied = bytesCopied >> 2;
            for (var i = 0; i < numIdsCopied; ++i)
            {
                var id = processIds[i];

                var handle = Kernel32.OpenProcess(ProcessAccess.QueryInformation, false, id);
                if (handle == IntPtr.Zero)
                    continue;

                Kernel32.CloseHandle(handle);

                Process process = null;
                try
                {
                    process = Process.GetProcessById(id);
                    if (process == null)
                        continue;
                }
                catch (Exception)
                {
                    continue;
                }

                yield return new RemoteProcess(process);
            }
        }

        public static RemoteProcess FindProcessById(int processId)
        {
            Process process;
            try
            {
                process = Process.GetProcessById(processId);
            }
            catch (Exception)
            {
                return null;
            }

            if (!Kernel32.Is32BitProcess(process.Handle))
                throw new MagicException("Can't operate with x64 processes");

            return new RemoteProcess(process);
        }

        #region String parameters methods
        public static IEnumerable<RemoteProcess> FindProcessesByInternalName(string name)
            => FindProcessesByInternalName(new Regex(Regex.Escape(name), RegexOptions.IgnoreCase));

        public static IEnumerable<RemoteProcess> FindProcessesByProductName(string name)
            => FindProcessesByProductName(new Regex(Regex.Escape(name), RegexOptions.IgnoreCase));

        public static IEnumerable<RemoteProcess> FindProcessesByName(string name)
            => FindProcessesByName(new Regex(Regex.Escape(name), RegexOptions.IgnoreCase));

        public static RemoteProcess FindProcessByName(string name)
            => FindProcessByName(new Regex(Regex.Escape(name), RegexOptions.IgnoreCase));

        public static RemoteProcess FindProcessByInternalName(string name)
            => FindProcessByInternalName(new Regex(Regex.Escape(name), RegexOptions.IgnoreCase));

        public static RemoteProcess FindProcessByProductName(string name)
            => FindProcessByProductName(new Regex(Regex.Escape(name), RegexOptions.IgnoreCase));

        public static RemoteProcess SelectProcess(string name)
            => SelectProcess(new Regex(Regex.Escape(name), RegexOptions.IgnoreCase));
        #endregion

        public static IEnumerable<RemoteProcess> FindProcessesByInternalName(Regex pattern)
        {
            return EnumerateProcesses().Where(process =>
            {
                try
                {
                    return process.IsValid && Kernel32.Is32BitProcess(process.Handle) &&
                        process.MainModule.FileVersionInfo.InternalName != null &&
                        pattern.IsMatch(process.MainModule.FileVersionInfo.InternalName);
                }
                catch (Win32Exception)
                {
                    return false;
                }
            });
        }

        public static IEnumerable<RemoteProcess> FindProcessesByProductName(Regex pattern)
        {
            return EnumerateProcesses().Where(process =>
            {
                return process.IsValid && Kernel32.Is32BitProcess(process.Handle) &&
                    process.MainModule.FileVersionInfo.ProductName != null &&
                    pattern.IsMatch(process.MainModule.FileVersionInfo.ProductName);
            });
        }

        public static IEnumerable<RemoteProcess> FindProcessesByName(Regex pattern)
        {
            return EnumerateProcesses().Where(process => 
                {
                    return process.IsValid && Kernel32.Is32BitProcess(process.Handle) &&
                        pattern.IsMatch(process.Name);
                });
        }

        public static RemoteProcess FindProcessByName(Regex pattern)
            => FindProcessesByName(pattern).FirstOrDefault();

        public static RemoteProcess FindProcessByInternalName(Regex pattern)
            => FindProcessesByInternalName(pattern).FirstOrDefault();

        public static RemoteProcess FindProcessByProductName(Regex pattern)
            => FindProcessesByProductName(pattern).FirstOrDefault();

        public static RemoteProcess SelectProcess(Regex pattern)
        {
            for (; ; )
            {
                var processList = FindProcessesByInternalName(pattern).ToList();
                if (processList.Count == 0)
                    throw new ProcessSelectorException("No processes found");

                try
                {
                    if (processList.Count == 1)
                        return processList[0];

                    Console.WriteLine("Select process:");
                    for (var i = 0; i < processList.Count; ++i)
                    {
                        var debugging = false;
                        Kernel32.CheckRemoteDebuggerPresent(processList[i].Handle, ref debugging);

                        Console.WriteLine("[{0}] {1} PID: {2} {3}",
                            i,
                            processList[i].GetVersionInfo(),
                            processList[i].Id,
                            debugging ? "(Already debugging)" : "");
                    }

                    Console.WriteLine();
                    Console.Write("> ");
                    var index = Convert.ToInt32(Console.ReadLine());

                    return processList[index];
                }
                catch (Exception ex)
                {
                    if (processList.Count == 1)
                        throw new ProcessSelectorException(ex.Message);
                }
            }
        }

        private const string SE_DEBUG_NAME = "SeDebugPrivilege";

        /// <summary>
        /// Sets debug privileges for running program.
        /// </summary>
        /// <returns>Internal name of a process</returns>
        public static bool SetDebugPrivileges()
        {
            if (!Advapi32.OpenProcessToken(Kernel32.GetCurrentProcess(), TokenObject.TOKEN_ADJUST_PRIVILEGES | TokenObject.TOKEN_QUERY, out var hToken))
                return false;

            if (!Advapi32.LookupPrivilegeValue(null, SE_DEBUG_NAME, out var luidSEDebugNameValue))
            {
                Kernel32.CloseHandle(hToken);
                return false;
            }

            TOKEN_PRIVILEGES tkpPrivileges;
            tkpPrivileges.PrivilegeCount = 1;
            tkpPrivileges.Luid = luidSEDebugNameValue;
            tkpPrivileges.Attributes = PrivilegeAttributes.SE_PRIVILEGE_ENABLED;

            if (!Advapi32.AdjustTokenPrivileges(hToken, false, ref tkpPrivileges, 0, IntPtr.Zero, IntPtr.Zero))
            {
                Kernel32.CloseHandle(hToken);
                return false;
            }

            return Kernel32.CloseHandle(hToken);
        }

        public enum ProcessStartFlags
        {
            None = 0,
            Suspended = 1,
            NoWindow = 2,
        }

        public class ProcessStartResult
        {
            public IntPtr ProcessHandle;
            public IntPtr MainThreadHandle;
            public int ProcessId;
            public int MainThreadId;
        }

        public static ProcessStartResult StartProcess(string filePath, string arguments = "",
            ProcessStartFlags startFlags = ProcessStartFlags.None)
        {
            if (!File.Exists(filePath))
                throw new MagicException($"No such file '{filePath}'");

            var sInfo = new STARTUPINFO();
            var pSec = new SECURITY_ATTRIBUTES();
            var tSec = new SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            var flags = CreateProcessFlags.DETACHED_PROCESS;
            if (startFlags.HasFlag(ProcessStartFlags.NoWindow))
                flags |= CreateProcessFlags.CREATE_NO_WINDOW;
            if (startFlags.HasFlag(ProcessStartFlags.Suspended))
                flags |= CreateProcessFlags.CREATE_SUSPENDED;

            if (!Kernel32.CreateProcess(filePath, arguments,
                ref pSec, ref tSec, false, flags,
                IntPtr.Zero, null, ref sInfo, out var pInfo))
                throw new MagicException("Failed to start process");

            return new ProcessStartResult()
            {
                ProcessId = pInfo.dwProcessId,
                ProcessHandle = pInfo.hProcess,
                MainThreadId = pInfo.dwThreadId,
                MainThreadHandle = pInfo.hThread
            };
        }
    }
}
