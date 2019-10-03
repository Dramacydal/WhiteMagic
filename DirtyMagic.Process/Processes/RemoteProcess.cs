using System;
using System.Collections.Generic;
using System.Diagnostics;
using DirtyMagic.WinAPI;

namespace DirtyMagic.Processes
{
    public class RemoteProcess
    {
        public Process Process { get; private set; }

        public int Id => Process.Id;

        public IntPtr Handle => Process.Handle;

        public RemoteWindow MainWindow => new RemoteWindow(this, MainWindowHandle);

        public IntPtr MainWindowHandle
        {
            get
            {
                if (!User32.IsWindow(Process.MainWindowHandle))
                    Process.Refresh();

                return Process.MainWindowHandle;
            }
        }

        public string Name => Process.ProcessName;

        public bool IsValid => !Process.HasExited;

        public bool Is32BitProcess => Kernel32.Is32BitProcess(Process.Handle);

        public void Refresh() => Process.Refresh();

        public ProcessModule MainModule => Process.MainModule;

        public IEnumerable<ProcessModule> Modules
        {
            get
            {
                foreach (ProcessModule module in Process.Modules)
                    yield return module;
            }
        }

        public IEnumerable<ProcessThread> Threads
        {
            get
            {
                foreach (ProcessThread thread in Process.Threads)
                    yield return thread;
            }
        }

        public static RemoteProcess GetById(int ProcessId)
        {
            return ProcessHelpers.FindProcessById(ProcessId);
        }

        public RemoteProcess(Process Process)
        {
            this.Process = Process;
        }

        public override string ToString()
        {
            return $"{Name} [{Id}]";
        }
    }
}
