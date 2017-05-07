using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures;
using WhiteMagic.WinAPI.Structures.Process;

namespace WhiteMagic
{
    public class ProcessSelectorException : Exception
    {
        public ProcessSelectorException(string message) : base(message) { }
    }

    public static class MagicHelpers
    {
        /// <summary>
        /// Use this as alternative to builtin Process.GetProcesses() in
        /// order not to deal with exceptions
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Process> EnumerateProcesses()
        {
            var arraySize = 1024u;
            var arrayBytesSize = arraySize * sizeof(uint);
            var processIds = new int[arraySize];
            uint bytesCopied;

            if (!Psapi.EnumProcesses(processIds, arrayBytesSize, out bytesCopied))
                yield break;

            if (bytesCopied == 0)
                yield break;

            if ((bytesCopied & 3) != 0)
                yield break;

            var numIdsCopied = bytesCopied >> 2;
            for (var i = 0; i < numIdsCopied; ++i)
            {
                var id = processIds[i];

                var Handle = Kernel32.OpenProcess(ProcessAccess.QueryInformation, false, id);
                if (Handle == IntPtr.Zero)
                    continue;

                Kernel32.CloseHandle(Handle);
                var process = Process.GetProcessById(id);
                if (process == null)
                    continue;

                yield return process;
            }
        }

        #region String parameters methods
        public static IEnumerable<Process> FindProcessesByInternalName(string Name)
            => FindProcessesByInternalName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));

        public static IEnumerable<Process> FindProcessesByProductName(string Name)
            => FindProcessesByProductName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));

        public static IEnumerable<Process> FindProcessesByName(string Name)
            => FindProcessesByName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));

        public static Process FindProcessByName(string Name)
            => FindProcessByName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));

        public static Process FindProcessByInternalName(string Name)
            => FindProcessByInternalName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));

        public static Process FindProcessByProductName(string Name)
            => FindProcessByProductName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));

        public static Process SelectProcess(string Name)
            => SelectProcess(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));
        #endregion

        public static IEnumerable<Process> FindProcessesByInternalName(Regex Pattern)
        {
            return EnumerateProcesses().Where(process =>
            {
                try
                {
                    return Kernel32.Is32BitProcess(process.Handle) &&
                        process.MainModule.FileVersionInfo.InternalName != null &&
                        Pattern.IsMatch(process.MainModule.FileVersionInfo.InternalName);
                }
                catch (Win32Exception)
                {
                    return false;
                }
            });
        }

        public static IEnumerable<Process> FindProcessesByProductName(Regex Pattern)
        {
            return EnumerateProcesses().Where(process =>
            {
                return Kernel32.Is32BitProcess(process.Handle) &&
                    process.MainModule.FileVersionInfo.ProductName != null &&
                    Pattern.IsMatch(process.MainModule.FileVersionInfo.ProductName);
            });
        }

        public static IEnumerable<Process> FindProcessesByName(Regex Pattern)
        {
            return EnumerateProcesses().Where(process => 
                {
                    return Kernel32.Is32BitProcess(process.Handle) &&
                        Pattern.IsMatch(process.ProcessName);
                });
        }

        public static Process FindProcessByName(Regex Pattern)
            => FindProcessesByName(Pattern).FirstOrDefault();

        public static Process FindProcessByInternalName(Regex Pattern)
            => FindProcessesByInternalName(Pattern).FirstOrDefault();

        public static Process FindProcessByProductName(Regex Pattern)
            => FindProcessesByProductName(Pattern).FirstOrDefault();

        public static Process SelectProcess(Regex Pattern)
        {
            for (; ; )
            {
                var processList = FindProcessesByInternalName(Pattern).ToList();
                if (processList.Count == 0)
                    throw new ProcessSelectorException("No processes found");

                try
                {
                    int index = 0;
                    if (processList.Count != 1)
                    {
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
                        index = Convert.ToInt32(Console.ReadLine());

                        return processList[index];
                    }
                    else
                        return processList[0];
                }
                catch (Exception ex)
                {
                    if (processList.Count == 1)
                        throw new ProcessSelectorException(ex.Message);
                    continue;
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
            IntPtr hToken;
            LUID luidSEDebugNameValue;
            TOKEN_PRIVILEGES tkpPrivileges;

            if (!Advapi32.OpenProcessToken(Kernel32.GetCurrentProcess(), TokenObject.TOKEN_ADJUST_PRIVILEGES | TokenObject.TOKEN_QUERY, out hToken))
                return false;

            if (!Advapi32.LookupPrivilegeValue(null, SE_DEBUG_NAME, out luidSEDebugNameValue))
            {
                Kernel32.CloseHandle(hToken);
                return false;
            }

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

        public static T ReinterpretObject<T>(object val) where T : struct
        {
            var h = GCHandle.Alloc(val, GCHandleType.Pinned);
            var t = (T)Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof(T));
            h.Free();

            return t;
        }

        public static byte[] ObjectToBytes<T>(T value)
        {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            return bytes;
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

        public static ProcessStartResult StartProcess(string FilePath, string Arguments = "", ProcessStartFlags StartFlags = ProcessStartFlags.None)
        {
            if (!File.Exists(FilePath))
                throw new MagicException("No such file '{0}'", FilePath);

            var pInfo = new PROCESS_INFORMATION();
            var sInfo = new STARTUPINFO();
            var pSec = new SECURITY_ATTRIBUTES();
            var tSec = new SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            var flags = CreateProcessFlags.DETACHED_PROCESS;
            if (StartFlags.HasFlag(ProcessStartFlags.NoWindow))
                flags |= CreateProcessFlags.CREATE_NO_WINDOW;
            if (StartFlags.HasFlag(ProcessStartFlags.Suspended))
                flags |= CreateProcessFlags.CREATE_SUSPENDED;

            if (!Kernel32.CreateProcess(FilePath, Arguments,
                ref pSec, ref tSec, false, flags,
                IntPtr.Zero, null, ref sInfo, out pInfo))
                throw new MagicException("Failed to start process");

            return new ProcessStartResult()
                {
                    ProcessId = pInfo.dwProcessId,
                    ProcessHandle = pInfo.hProcess,
                    MainThreadId = pInfo.dwThreadId,
                    MainThreadHandle= pInfo.hThread
                };
        }
    }
}
