using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32HWBP
{
    public class BreakPointException : Exception
    {
        public BreakPointException(string message) : base(message) { }
    }

    public class HardwareBreakPoint
    {
        public enum Condition
        {
            Code = 0,
            Write = 1,
            ReadWrite = 2,
        }

        public HardwareBreakPoint(int address, uint len, Condition condition)
        {
            this.address = address;
            this.condition = condition;

            switch (len)
            {
                case 1: this.len = 0; break;
                case 2: this.len = 1; break;
                case 4: this.len = 3; break;
                case 8: this.len = 2; break;
                default: throw new BreakPointException("Invalid length!");
            }
        }

        public void Set(uint threadId)
        {
            // make sure this breakpoint isn't already set
            if (m_index != -1)
                throw new BreakPointException("Breakpoint is already set!");

            this.threadId = threadId;

            var cxt = new CONTEXT();

            // The only registers we care about are the debug registers
            cxt.ContextFlags = (uint)CONTEXT_FLAGS.CONTEXT_DEBUG_REGISTERS;

            var hThread = WinApi.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, threadId);
            if (hThread == IntPtr.Zero)
                throw new BreakPointException("Can't open thread for access");

            if (WinApi.SuspendThread(hThread) == -1)
                throw new BreakPointException("Can't suspend thread");

            // Read the register values
            if (!WinApi.GetThreadContext(hThread, ref cxt))
                throw new BreakPointException("Failed to get thread context");

            // Find an available hardware register
            for (m_index = 0; m_index < WinApi.MAX_BREAKPOINTS; ++m_index)
            {
                if ((cxt.Dr7 & (1 << (m_index * 2))) == 0)
                    break;
            }

            if (m_index == WinApi.MAX_BREAKPOINTS)
                throw new BreakPointException("All hardware breakpoint registers are already being used");

            switch (m_index)
            {
                case 0: cxt.Dr0 = (uint)address; break;
                case 1: cxt.Dr1 = (uint)address; break;
                case 2: cxt.Dr2 = (uint)address; break;
                case 3: cxt.Dr3 = (uint)address; break;
                default: throw new BreakPointException("m_index has bogus value!");
            }

            SetBits(ref cxt.Dr7, 16 + (m_index * 4), 2, (uint)condition);
            SetBits(ref cxt.Dr7, 18 + (m_index * 4), 2, len);
            SetBits(ref cxt.Dr7, m_index * 2, 1, 1);

            // Write out the new debug registers
            if (!WinApi.SetThreadContext(hThread, ref cxt))
                throw new BreakPointException("Failed to set thread context");

            if (WinApi.ResumeThread(hThread) == -1)
                throw new BreakPointException("Failed to resume thread");
        }

        public void UnSet()
        {
            if (m_index == -1 || threadId == 0)
                return;

            // Zero out the debug register settings for this breakpoint
            if (m_index >= WinApi.MAX_BREAKPOINTS)
                throw new BreakPointException("Bogus breakpoints index");

            var cxt = new CONTEXT();
            // The only registers we care about are the debug registers
            cxt.ContextFlags = (uint)CONTEXT_FLAGS.CONTEXT_DEBUG_REGISTERS;

            var hThread = WinApi.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, threadId);
            if (hThread == IntPtr.Zero)
                throw new BreakPointException("Can't open thread for access");

            if (WinApi.SuspendThread(hThread) == -1)
                throw new BreakPointException("Can't suspend thread");

            // Read the register values
            if (!WinApi.GetThreadContext(hThread, ref cxt))
                throw new BreakPointException("Failed to get thread context");

            SetBits(ref cxt.Dr7, m_index * 2, 1, 0);

            // Write out the new debug registers
            if (!WinApi.SetThreadContext(hThread, ref cxt))
                throw new BreakPointException("Failed to set thread context");

            if (WinApi.ResumeThread(hThread) == -1)
                throw new BreakPointException("Failed to resume thread");

            m_index = -1;
        }

        public bool OnEvent(ref DEBUG_EVENT DebugEvent/*, ref ProcessDebugger pd*/)
        {
            return false;
        }

        public virtual bool HandleException(ref CONTEXT ctx/*, ref ProcessDebugger pd*/) { return false; }

        public void SetBits(ref uint dw, int lowBit, int bits, uint newValue)
        {
            var mask = (1u << bits) - 1; // e.g. 1 becomes 0001, 2 becomes 0011, 3 becomes 0111
            dw = (dw & ~(mask << lowBit)) | (newValue << lowBit);
        }

        public void Shift(uint offset, bool set = false)
        {
            if (set)
                address = (int)offset;
            else
                address += (int)offset;
        }

        public int Index { get { return m_index; } }
        public uint Address { get { return (uint)address; } }

        protected int m_index = -1;
        protected int address;

        protected readonly uint len;
        protected readonly Condition condition;
        protected uint threadId = 0;
    }
}
