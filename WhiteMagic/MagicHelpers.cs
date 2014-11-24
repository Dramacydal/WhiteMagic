using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using WhiteMagic.WinAPI;

namespace WhiteMagic
{
    public static class MagicHelpers
    {
        /// <summary>
        /// Generates list of processes by internal name specified
        /// </summary>
        /// <param name="name">Internal name of a process</param>
        /// <returns></returns>
        public static List<Process> FindProcessesByInternalName(string name)
        {
            var list = new List<Process>();
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.MainModule.FileVersionInfo.InternalName.ToLower() == name.ToLower())
                        list.Add(process);
                }
                catch (NullReferenceException)
                {
                }
                catch (Win32Exception)
                {
                }
            }

            return list;
        }

        /// <summary>
        /// Generates list of processes by name specified
        /// </summary>
        /// <param name="name">Name of process executable</param>
        /// <returns>List of matching processes</returns>
        public static List<Process> FindProcessesByName(string name)
        {
            var list = new List<Process>();
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.MainModule.ModuleName.ToLower() == name.ToLower())
                        list.Add(process);
                }
                catch (NullReferenceException)
                {
                }
                catch (Win32Exception)
                {
                }
            }

            return list;
        }

        /// <summary>
        /// Returns first found process with name
        /// </summary>
        /// <param name="name">Name of process executable</param>
        /// <returns></returns>
        public static Process FindProcessByName(string name)
        {
            var processes = FindProcessesByName(name);

            return processes.Count == 0 ? null : processes[0];
        }

        /// <summary>
        /// Returns first found process with internal name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Process FindProcessByInternalName(string name)
        {
            var processes = FindProcessesByInternalName(name);
            return processes.Count == 0 ? null : processes[0];
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
    }
}
