using System;
using System.Collections.Generic;
using System.Diagnostics;
using WhiteMagic.Patterns;

namespace WhiteMagic.Modules
{
    public class ModuleDump
    {
        public IntPtr BaseAddress { get; private set; }
        public int ModuleSize { get { return MemoryDump.Length; } }
        public byte[] MemoryDump { get; private set; }

        private static readonly int readCount = 256;

        public ModuleDump(ProcessModule module, MemoryHandler m)
        {
            var bytes = new List<byte>();
            for (var i = 0; i < module.ModuleMemorySize; i += readCount)
                bytes.AddRange(m.ReadBytes(IntPtr.Add(module.BaseAddress, i), i + readCount >= module.ModuleMemorySize ? module.ModuleMemorySize - i - 1 : readCount));

            BaseAddress = module.BaseAddress;
            MemoryDump = bytes.ToArray();
        }

        public IntPtr Find(MemoryPattern pattern, int startOffset)
        {
            var offs = pattern.Find(MemoryDump, startOffset);
            if (offs == int.MaxValue)
                return new IntPtr(offs);

            return IntPtr.Add(BaseAddress, offs);
        }

        public IntPtr FindNext(MemoryPattern pattern)
        {
            var offs = pattern.FindNext(MemoryDump);
            if (offs == int.MaxValue)
                return new IntPtr(offs);

            return IntPtr.Add(BaseAddress, offs);
        }
    }
}
