using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteMagic.WinAPI;

namespace WhiteMagic.Processes
{
    public class RemoteProcess
    {
        public Process Process { get; private set; }

        public bool IsValid => !Process.HasExited;

        public bool Is32BitProcess => Kernel32.Is32BitProcess(Process.Handle);

        public RemoteProcess(int ProcessId)
        {
            var Process = MagicHelpers.FindProcessById(ProcessId);
            if (Process == null)
                throw new MagicException($"Process with id {ProcessId} not found");

            this.Process = Process;
        }

        public RemoteProcess(Process Process)
        {
            this.Process = Process;
        }
    }
}
