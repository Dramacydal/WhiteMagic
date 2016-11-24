using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures;

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
            var arrayBytesSize = arraySize * sizeof(UInt32);
            var processIds = new int[arraySize];
            uint bytesCopied;

            if (!Psapi.EnumProcesses(processIds, arrayBytesSize, out bytesCopied))
                yield break;

            if (bytesCopied == 0)
                yield break;

            if ((bytesCopied & 3) != 0)
                yield break;

            UInt32 numIdsCopied = bytesCopied >> 2;
            for (UInt32 i = 0; i < numIdsCopied; ++i)
            {
                var id = processIds[i];

                var Handle = Kernel32.OpenProcess(ProcessAccess.QueryInformation, false, (int)id);
                if (Handle == IntPtr.Zero)
                    continue;

                Kernel32.CloseHandle(Handle);
                var process = Process.GetProcessById((int)id);
                if (process == null)
                    continue;

                yield return process;
            }
        }

        #region String parameters methods
        public static IEnumerable<Process> FindProcessesByInternalName(string Name)
        {
            return FindProcessesByInternalName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));
        }

        public static IEnumerable<Process> FindProcessesByProductName(string Name)
        {
            return FindProcessesByProductName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));
        }

        public static IEnumerable<Process> FindProcessesByName(string Name)
        {
            return FindProcessesByName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));
        }

        public static Process FindProcessByName(string Name)
        {
            return FindProcessByName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));
        }

        public static Process FindProcessByInternalName(string Name)
        {
            return FindProcessByInternalName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));
        }

        public static Process FindProcessByProductName(string Name)
        {
            return FindProcessByProductName(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));
        }

        public static Process SelectProcess(string Name)
        {
            return SelectProcess(new Regex(Regex.Escape(Name), RegexOptions.IgnoreCase));
        }
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
        {
            var processes = FindProcessesByName(Pattern);
            return processes.FirstOrDefault();
        }

        public static Process FindProcessByInternalName(Regex Pattern)
        {
            var processes = FindProcessesByInternalName(Pattern);
            return processes.FirstOrDefault();
        }

        public static Process FindProcessByProductName(Regex Pattern)
        {
            var processes = FindProcessesByProductName(Pattern);
            return processes.FirstOrDefault();
        }

        public static Process SelectProcess(Regex Pattern)
        {
            for (; ; )
            {
                var processList = FindProcessesByInternalName(Pattern).ToList();
                if (processList.Count == 0)
                    throw new ProcessSelectorException(string.Format("No processes found"));

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
    }
}
