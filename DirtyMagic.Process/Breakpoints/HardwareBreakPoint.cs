using System;
using System.Collections.Generic;
using DirtyMagic.Exceptions;
using DirtyMagic.Pointers;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Breakpoints
{
    [Flags]
    public enum SlotFlags : int
    {
        None = 0,
        Bp0 = 1,
        Bp1 = 2,
        Bp2 = 4,
        Bp3 = 8,
        All = Bp0 | Bp1 | Bp2 | Bp3
    }

    public abstract class HardwareBreakPoint
    {
        public BreakpointCondition Condition { get; }
        public bool IsSet => Address != IntPtr.Zero;
        public IntPtr Address { get; private set; } = IntPtr.Zero;

        public ModulePointer Pointer { get; }

        
        protected MemoryHandler Memory { get; private set; }

        private readonly Dictionary<int, int> _affectedThreads = new Dictionary<int, int>();

        protected readonly int Length;

        protected HardwareBreakPoint(ModulePointer Pointer, BreakpointCondition Condition, int Length)
        {
            if (Condition == BreakpointCondition.Code)
                Length = 1;

            this.Pointer = Pointer;
            this.Condition = Condition;

            switch (Length)
            {
                case 1: this.Length = 0; break;
                case 2: this.Length = 1; break;
                case 4: this.Length = 3; break;
                case 8: this.Length = 2; break;
                default: throw new BreakPointException($"Invalid breakpoint length! ({Length})");
            }
        }

        public bool Set(MemoryHandler memory)
        {
            this.Memory = memory;
            memory.RefreshMemory();

            Address = memory.GetAddress(Pointer);
            if (Address == null)
                return false;

            foreach (var th in memory.Process.Threads)
            {
                if (_affectedThreads.ContainsKey(th.Id))
                    continue;

                var hThread = Kernel32.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, th.Id);
                if (hThread == IntPtr.Zero)
                    throw new BreakPointException("Can't open thread for access");

                SetToThread(hThread, th.Id);

                if (!Kernel32.CloseHandle(hThread))
                    throw new BreakPointException("Failed to close thread handle");
            }

            return true;
        }

        public void SetToThread(IntPtr threadHandle, int threadId)
        {
            // make sure this breakpoint isn't already set
            if (_affectedThreads.ContainsKey(threadId))
                return;

            var cxt = new CONTEXT();

            // The only registers we care about are the debug registers
            cxt.ContextFlags = CONTEXT_FLAGS.CONTEXT_DEBUG_REGISTERS;

            // Read the register values
            if (!Kernel32.GetThreadContext(threadHandle, cxt))
                throw new BreakPointException("Failed to get thread context");

            // Find an available hardware register
            var index = cxt.GetFreeBreakpointSlot();

            if (index == -1)
                throw new BreakPointException("All hardware breakpoint registers are already being used");

            switch (index)
            {
                case 0: cxt.Dr0 = Address.ToUInt32(); break;
                case 1: cxt.Dr1 = Address.ToUInt32(); break;
                case 2: cxt.Dr2 = Address.ToUInt32(); break;
                case 3: cxt.Dr3 = Address.ToUInt32(); break;
                default: throw new BreakPointException("m_index has bogus value!");
            }

            SetBits(ref cxt.Dr7, 16 + index * 4, 2, (uint)Condition);
            SetBits(ref cxt.Dr7, 18 + index * 4, 2, (uint)Length);
            SetBits(ref cxt.Dr7, index * 2, 1, 1);

            // Write out the new debug registers
            if (!Kernel32.SetThreadContext(threadHandle, cxt))
                throw new BreakPointException("Failed to set thread context");

            _affectedThreads[threadId] = index;
        }

        public void UnregisterThread(int ThreadId) => _affectedThreads.Remove(ThreadId);

        public void UnSet(MemoryHandler memory)
        {
            memory.RefreshMemory();

            foreach (var th in memory.Process.Threads)
            {
                if (!_affectedThreads.ContainsKey(th.Id))
                    continue;

                var hThread = Kernel32.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, th.Id);
                if (hThread == IntPtr.Zero)
                    throw new BreakPointException("Can't open thread for access");

                UnsetFromThread(hThread, th.Id);

                if (!Kernel32.CloseHandle(hThread))
                    throw new BreakPointException("Failed to close thread handle");
            }

            _affectedThreads.Clear();
        }

        public void UnsetFromThread(IntPtr threadHandle, int threadId)
        {
            var index = _affectedThreads[threadId];
            // Zero out the debug register settings for this breakpoint
            if (index >= Kernel32.MaxHardwareBreakpointsCount)
                throw new BreakPointException("Bogus breakpoints index");

            UnsetSlotsFromThread(threadHandle, (SlotFlags)(1 << index));
        }

        public static void UnsetSlotsFromThread(IntPtr threadHandle, SlotFlags slotMask)
        {
            var cxt = new CONTEXT();
            // The only registers we care about are the debug registers
            cxt.ContextFlags = CONTEXT_FLAGS.CONTEXT_DEBUG_REGISTERS;

            // Read the register values
            if (!Kernel32.GetThreadContext(threadHandle, cxt))
                throw new BreakPointException("Failed to get thread context");

            for (var i = 0; i < Kernel32.MaxHardwareBreakpointsCount; ++i)
                if (slotMask.HasFlag((SlotFlags)(1 << i)))
                    SetBits(ref cxt.Dr7, i * 2, 1, 0);

            // Write out the new debug registers
            if (!Kernel32.SetThreadContext(threadHandle, cxt))
                throw new BreakPointException("Failed to set thread context");
        }

        /// <summary>
        /// Handles breakpoint trigger
        /// </summary>
        /// <param name="wrapper"></param>
        /// <returns>If true, modified context will be set to thread</returns>
        public abstract bool HandleException(ContextWrapper wrapper);

        protected static void SetBits(ref uint dw, int lowBit, int bits, uint newValue)
        {
            var mask = (1u << bits) - 1; // e.g. 1 becomes 0001, 2 becomes 0011, 3 becomes 0111
            dw = (dw & ~(mask << lowBit)) | (newValue << lowBit);
        }
    }
}
