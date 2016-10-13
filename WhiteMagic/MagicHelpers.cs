using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
        public static List<Process> FindProcessesByInternalName(string name)
        {
            var processes = new List<Process>(Process.GetProcesses());
            return processes.Where(process =>
            {
                try
                {
                    return Kernel32.Is32BitProcess(process.Handle) &&
                        process.MainModule.FileVersionInfo.InternalName != null &&
                        process.MainModule.FileVersionInfo.InternalName.ToLower() == name.ToLower();
                }
                catch (Win32Exception)
                {
                    return false;
                }
            }).ToList();
        }

        public static List<Process> FindProcessesByProductName(string name)
        {
            var processes = new List<Process>(Process.GetProcesses());
            return processes.Where(process =>
            {
                return Kernel32.Is32BitProcess(process.Handle) &&
                    process.MainModule.FileVersionInfo.ProductName != null &&
                    process.MainModule.FileVersionInfo.ProductName.ToLower() == name.ToLower();
            }).ToList();
        }

        public static List<Process> FindProcessesByName(string name)
        {
            var processes = new List<Process>(Process.GetProcessesByName(name));
            return processes.Where(process => Kernel32.Is32BitProcess(process.Handle)).ToList();
        }

        public static Process FindProcessByName(string name)
        {
            var processes = FindProcessesByName(name);
            return processes.Count == 0 ? null : processes[0];
        }

        public static Process FindProcessByInternalName(string name)
        {
            var processes = FindProcessesByInternalName(name);
            return processes.Count == 0 ? null : processes[0];
        }

        public static Process FindProcessByProductName(string name)
        {
            var processes = FindProcessesByProductName(name);
            return processes.Count == 0 ? null : processes[0];
        }

        public static Process SelectProcess(string internalName)
        {
            for (; ; )
            {
                var processList = FindProcessesByInternalName(internalName);
                if (processList.Count == 0)
                    throw new ProcessSelectorException(string.Format("No '{0}' processes found", internalName));

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

        private static string SE_DEBUG_NAME = "SeDebugPrivilege";

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
