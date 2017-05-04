using System;

namespace WhiteMagic
{
    public class ProcessSuspender : IDisposable
    {
        private MemoryHandler Memory;

        public ProcessSuspender(MemoryHandler Memory)
        {
            this.Memory = Memory;
            Memory.SuspendAllThreads();
        }

        public void Dispose()
        {
            Memory.ResumeAllThreads();
        }
    }
}
