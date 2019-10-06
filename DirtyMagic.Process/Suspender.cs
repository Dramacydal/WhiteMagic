using System;

namespace DirtyMagic
{
    public class ProcessSuspender : IDisposable
    {
        private readonly MemoryHandler _memory;

        public ProcessSuspender(MemoryHandler memory)
        {
            _memory = memory;
            memory.SuspendAllThreads();
        }

        public void Dispose()
        {
            _memory.ResumeAllThreads();
        }
    }
}
