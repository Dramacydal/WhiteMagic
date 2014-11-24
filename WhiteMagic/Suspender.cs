using System;

namespace WhiteMagic
{
    public class ProcessSuspender : IDisposable
    {
        private MemoryHandler m;

        public ProcessSuspender(MemoryHandler m)
        {
            this.m = m;
            m.SuspendAllThreads();
        }

        public void Dispose()
        {
            m.ResumeAllThreads();
        }
    }
}
