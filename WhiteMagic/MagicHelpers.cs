using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using WhiteMagic.WinAPI;

namespace WhiteMagic
{
    public class ProcessSelectorException : Exception
    {
        public ProcessSelectorException(string message) : base(message) { }
    }

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

        public static List<Process> FindProcessesByProductName(string name)
        {
            var list = new List<Process>();
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.MainModule.FileVersionInfo.ProductName.ToLower() == name.ToLower())
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
    }
}
