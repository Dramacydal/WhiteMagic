using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

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
                if (process.MainModule.ModuleName.ToLower() == name.ToLower())
                    list.Add(process);
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
    }
}
