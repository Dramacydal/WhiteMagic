using System;
using System.Collections.Generic;
using System.Diagnostics;
using WhiteMagic.WinAPI;

namespace WhiteMagic
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

        public HardwareBreakPoint(IntPtr address, uint len, Condition condition)
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

        public void Set(Process p)
        {
            process = p;
            process.Refresh();

            foreach (ProcessThread th in p.Threads)
            {
                if (affectedThreads.ContainsKey(th.Id))
                    continue;

                // make sure this breakpoint isn't already set
                var cxt = new CONTEXT();

                // The only registers we care about are the debug registers
                cxt.ContextFlags = (uint)CONTEXT_FLAGS.CONTEXT_DEBUG_REGISTERS;

                var hThread = Kernel32.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, th.Id);
                if (hThread == IntPtr.Zero)
                    throw new BreakPointException("Can't open thread for access");

                // Read the register values
                if (!Kernel32.GetThreadContext(hThread, ref cxt))
                    throw new BreakPointException("Failed to get thread context");

                // Find an available hardware register
                var index = -1;
                for (index = 0; index < Kernel32.MaxHardwareBreakpoints; ++index)
                {
                    if ((cxt.Dr7 & (1 << (index * 2))) == 0)
                        break;
                }

                if (index == Kernel32.MaxHardwareBreakpoints)
                    throw new BreakPointException("All hardware breakpoint registers are already being used");

                switch (index)
                {
                    case 0: cxt.Dr0 = (uint)address; break;
                    case 1: cxt.Dr1 = (uint)address; break;
                    case 2: cxt.Dr2 = (uint)address; break;
                    case 3: cxt.Dr3 = (uint)address; break;
                    default: throw new BreakPointException("m_index has bogus value!");
                }

                SetBits(ref cxt.Dr7, 16 + (index * 4), 2, (uint)condition);
                SetBits(ref cxt.Dr7, 18 + (index * 4), 2, (uint)len);
                SetBits(ref cxt.Dr7, index * 2, 1, 1);

                // Write out the new debug registers
                if (!Kernel32.SetThreadContext(hThread, ref cxt))
                    throw new BreakPointException("Failed to set thread context");

                if (!Kernel32.CloseHandle(hThread))
                    throw new BreakPointException("Failed to close thread handle");

                affectedThreads[th.Id] = index;
            }
        }

        public void UnSet()
        {
            if (process == null)
                return;

            process.Refresh();
            foreach (ProcessThread th in process.Threads)
            {
                if (!affectedThreads.ContainsKey(th.Id))
                    continue;

                var index = affectedThreads[th.Id];
                // Zero out the debug register settings for this breakpoint
                if (index >= Kernel32.MaxHardwareBreakpoints)
                    throw new BreakPointException("Bogus breakpoints index");

                var cxt = new CONTEXT();
                // The only registers we care about are the debug registers
                cxt.ContextFlags = (uint)CONTEXT_FLAGS.CONTEXT_DEBUG_REGISTERS;

                var hThread = Kernel32.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, th.Id);
                if (hThread == IntPtr.Zero)
                    throw new BreakPointException("Can't open thread for access");

                // Read the register values
                if (!Kernel32.GetThreadContext(hThread, ref cxt))
                    throw new BreakPointException("Failed to get thread context");

                SetBits(ref cxt.Dr7, index * 2, 1, 0);

                // Write out the new debug registers
                if (!Kernel32.SetThreadContext(hThread, ref cxt))
                    throw new BreakPointException("Failed to set thread context");

                if (!Kernel32.CloseHandle(hThread))
                    throw new BreakPointException("Failed to close thread handle");
            }

            affectedThreads.Clear();
        }

        public virtual bool HandleException(ref CONTEXT ctx, ProcessDebugger pd) { return false; }

        protected void SetBits(ref uint dw, int lowBit, int bits, uint newValue)
        {
            var mask = (1u << bits) - 1; // e.g. 1 becomes 0001, 2 becomes 0011, 3 becomes 0111
            dw = (dw & ~(mask << lowBit)) | (newValue << lowBit);
        }

        public void Shift(int offset, bool set = false)
        {
            if (set)
                address = new IntPtr(offset);
            else
                address = IntPtr.Add(address, offset);
        }

        public IntPtr Address { get { return address; } }

        Dictionary<int, int> affectedThreads = new Dictionary<int, int>();
        protected IntPtr address;

        protected readonly int len;
        protected readonly Condition condition;

        protected Process process = null;
    }
}
