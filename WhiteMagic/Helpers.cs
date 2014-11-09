using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using WhiteMagic.WinAPI;

namespace WhiteMagic
{
    public static class Helpers
    {
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

        public static string SE_DEBUG_NAME = "SeDebugPrivilege";

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

            Kernel32.CloseHandle(hToken);
            return true;
        }
    }
}
