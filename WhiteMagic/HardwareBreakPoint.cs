using System;
using System.Collections.Generic;
using System.Diagnostics;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Types;

namespace WhiteMagic
{
    public class BreakPointException : MagicException
    {
        public BreakPointException(string message) : base(message) { }
    }

    public enum ArchitectureType
    {
        x86 = 0,
        x64 = 1
    }

    public abstract class HardwareBreakPoint
    {
        public HardwareBreakPoint(ArchitectureType architecture, IntPtr offset, int len, BreakpointCondition condition)
        {
            Architecture = architecture;
            Offset = offset;
            Condition = condition;

            switch (len)
            {
                case 1: this.Length = 0; break;
                case 2: this.Length = 1; break;
                case 4: this.Length = 3; break;
                case 8: this.Length = 2; break;
                default: throw new BreakPointException("Invalid length!");
            }
        }

        public void Set(Process p)
        {
            process = p;
            process.Refresh();

            if (process.HasExited)
                throw new BreakPointException("Process has exited");

            if (process.GetArchitecture() != Architecture)
                throw new BreakPointException("Process and breakpoint architecture mismatch");

            foreach (ProcessThread th in p.Threads)
            {
                if (affectedThreads.ContainsKey(th.Id))
                    continue;

                var hThread = Kernel32.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, th.Id);
                if (hThread == IntPtr.Zero)
                    throw new BreakPointException("Can't open thread for access");

                SetToThread(hThread, th.Id);

                if (!Kernel32.CloseHandle(hThread))
                    throw new BreakPointException("Failed to close thread handle");
            }
        }

        protected int SetContext_x86(IntPtr hThread)
        {
            var ctx = new CONTEXT();

            // The only registers we care about are the debug registers
            ctx.ContextFlags = (uint)CONTEXT_FLAGS.CONTEXT_DEBUG_REGISTERS;

            // Read the register values
            if (!Kernel32.GetThreadContext(hThread, ref ctx))
                throw new BreakPointException("Failed to get thread context");

            // Find an available hardware register
            var index = ctx.GetFreeBreakpointSlot();

            if (index == -1)
                throw new BreakPointException("All hardware breakpoint registers are already being used");

            switch (index)
            {
                case 0: ctx.Dr0 = Address.ToUInt32(); break;
                case 1: ctx.Dr1 = Address.ToUInt32(); break;
                case 2: ctx.Dr2 = Address.ToUInt32(); break;
                case 3: ctx.Dr3 = Address.ToUInt32(); break;
                default: throw new BreakPointException("m_index has bogus value!");
            }

            ctx.Dr7 = SetBits(ctx.Dr7, 16 + index * 4, 2, (uint)Condition);
            ctx.Dr7 = SetBits(ctx.Dr7, 18 + index * 4, 2, (uint)Length);
            ctx.Dr7 = SetBits(ctx.Dr7, index * 2, 1, 1);

            // Write out the new debug registers
            if (!Kernel32.SetThreadContext(hThread, ref ctx))
                throw new BreakPointException("Failed to set thread context");

            return index;
        }

        protected int SetContext_x64(IntPtr hThread)
        {
            var ctx = new CONTEXT_x64();

            // The only registers we care about are the debug registers
            ctx.ContextFlags = (uint)CONTEXT_FLAGS_x64.CONTEXT_DEBUG_REGISTERS;

            // Read the register values
            if (!Kernel32.GetThreadContext(hThread, ref ctx))
                throw new BreakPointException("Failed to get thread context");

            // Find an available hardware register
            var index = ctx.GetFreeBreakpointSlot();

            if (index == -1)
                throw new BreakPointException("All hardware breakpoint registers are already being used");

            switch (index)
            {
                case 0: ctx.Dr0 = Address.ToUInt64(); break;
                case 1: ctx.Dr1 = Address.ToUInt64(); break;
                case 2: ctx.Dr2 = Address.ToUInt64(); break;
                case 3: ctx.Dr3 = Address.ToUInt64(); break;
                default: throw new BreakPointException("m_index has bogus value!");
            }

            ctx.Dr7 = SetBits((uint)ctx.Dr7, 16 + index * 4, 2, (uint)Condition);
            ctx.Dr7 = SetBits((uint)ctx.Dr7, 18 + index * 4, 2, (uint)Length);
            ctx.Dr7 = SetBits((uint)ctx.Dr7, index * 2, 1, 1);

            // Write out the new debug registers
            if (!Kernel32.SetThreadContext(hThread, ref ctx))
                throw new BreakPointException("Failed to set thread context");

            return index;
        }

        public void SetToThread(IntPtr hThread, int threadId)
        {
            // make sure this breakpoint isn't already set
            if (affectedThreads.ContainsKey(threadId))
                return;
            
            //Console.WriteLine("Thread {0} already affected", threadId);

            affectedThreads[threadId] = Is32Bit ? SetContext_x86(hThread) : SetContext_x64(hThread);
        }

        public void UnregisterThread(int id)
        {
            affectedThreads.Remove(id);
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

                var hThread = Kernel32.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, th.Id);
                if (hThread == IntPtr.Zero)
                    throw new BreakPointException("Can't open thread for access");

                UnsetFromThread(hThread, th.Id);

                if (!Kernel32.CloseHandle(hThread))
                    throw new BreakPointException("Failed to close thread handle");
            }

            affectedThreads.Clear();
        }

        public void UnsetFromThread(IntPtr hThread, int threadId)
        {
            var index = affectedThreads[threadId];
            // Zero out the debug register settings for this breakpoint
            if (index >= Kernel32.MaxHardwareBreakpoints)
                throw new BreakPointException("Bogus breakpoints index");

            if (Is32Bit)
                UnsetContext_x86(hThread, 1 << index);
            else
                UnsetContext_x64(hThread, 1 << index);
        }

        public static void UnsetContext_x86(IntPtr hThread, int slotMask)
        {
            var ctx = new CONTEXT();
            // The only registers we care about are the debug registers
            ctx.ContextFlags = (uint)CONTEXT_FLAGS.CONTEXT_DEBUG_REGISTERS;

            // Read the register values
            if (!Kernel32.GetThreadContext(hThread, ref ctx))
                throw new BreakPointException("Failed to get thread context");

            for (var i = 0; i < Kernel32.MaxHardwareBreakpoints; ++i)
                if ((slotMask & (1 << i)) != 0)
                    ctx.Dr7 = SetBits(ctx.Dr7, i * 2, 1, 0);

            // Write out the new debug registers
            if (!Kernel32.SetThreadContext(hThread, ref ctx))
                throw new BreakPointException("Failed to set thread context");
        }

        public static void UnsetContext_x64(IntPtr hThread, int slotMask)
        {
            var ctx = new CONTEXT_x64();
            // The only registers we care about are the debug registers
            ctx.ContextFlags = (uint)CONTEXT_FLAGS_x64.CONTEXT_DEBUG_REGISTERS;

            // Read the register values
            if (!Kernel32.GetThreadContext(hThread, ref ctx))
                throw new BreakPointException("Failed to get thread context");

            for (var i = 0; i < Kernel32.MaxHardwareBreakpoints; ++i)
                if ((slotMask & (1 << i)) != 0)
                    ctx.Dr7 = SetBits((uint)ctx.Dr7, i * 2, 1, 0);

            // Write out the new debug registers
            if (!Kernel32.SetThreadContext(hThread, ref ctx))
                throw new BreakPointException("Failed to set thread context");
        }

        public virtual bool HandleException(ref CONTEXT ctx, ProcessDebugger pd) { return false; }
        public virtual bool HandleException(ref CONTEXT_x64 ctx, ProcessDebugger pd) { return false; }

        protected static uint SetBits(uint dw, int lowBit, int bits, uint newValue)
        {
            var mask = (1u << bits) - 1; // e.g. 1 becomes 0001, 2 becomes 0011, 3 becomes 0111
            return (dw & ~(mask << lowBit)) | (newValue << lowBit);
        }

        public void SetModuleBase(IntPtr moduleBase)
        {
            ModuleBase = moduleBase;
        }

        public IntPtr ModuleBase { get; protected set; }
        public IntPtr Offset { get; protected set; }
        public IntPtr Address { get { return ModuleBase.Add(Offset); } }
        public BreakpointCondition Condition { get; protected set; }

        Dictionary<int, int> affectedThreads = new Dictionary<int, int>();

        protected readonly int Length;
        protected Process process = null;
        protected ArchitectureType Architecture { get; private set; }
        public bool Is32Bit { get { return Architecture == ArchitectureType.x86; } }
    }
}
