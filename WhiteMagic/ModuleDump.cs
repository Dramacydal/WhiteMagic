using System.Collections.Generic;
using System.Diagnostics;

namespace WhiteMagic
{
    public class ModuleDump
    {
        public uint BaseAddress { get; set; }
        public int ModuleSize { get { return MemoryDump.Length; } }
        public byte[] MemoryDump { get; set; }

        private static readonly int readCount = 256;

        public ModuleDump(ProcessModule module, MemoryHandler m)
        {
            var bytes = new List<byte>();
            for (var i = 0; i < module.ModuleMemorySize; i += readCount)
                bytes.AddRange(m.ReadBytes((uint)(module.BaseAddress + i), i + readCount >= module.ModuleMemorySize ? module.ModuleMemorySize - i - 1 : readCount));

            BaseAddress = (uint)module.BaseAddress;
            MemoryDump = bytes.ToArray();
        }

        public uint Find(MemoryPattern pattern, int startAddress = 0)
        {
            var offs = pattern.Find(MemoryDump, startAddress);
            if (offs == uint.MaxValue)
                return uint.MaxValue;

            return offs + BaseAddress;
        }

        public uint FindNext(MemoryPattern pattern)
        {
            var offs = pattern.FindNext(MemoryDump);
            if (offs == uint.MaxValue)
                return uint.MaxValue;

            return offs + BaseAddress;
        }
    }
}
