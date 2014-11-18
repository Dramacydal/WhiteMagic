using System;

namespace WhiteMagic
{
    public class Suspender : IDisposable
    {
        private MemoryHandler m;

        public Suspender(MemoryHandler m)
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
