using System.Collections.Generic;
using System.Diagnostics;

namespace WhiteMagic
{
    public class ModuleDump
    {
        public uint BaseAddress { get; set; }
        public uint ModuleSize { get { return (uint)MemoryDump.Length; } }
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

        public uint FindPattern(BytePattern pattern, uint startAddress = 0)
        {
            if (startAddress >= BaseAddress)
                startAddress -= BaseAddress;

            for (uint i = startAddress; i < ModuleSize; ++i)
            {
                var cont = false;
                for (uint j = 0; j < pattern.Length; ++j)
                {
                    if (i + j >= ModuleSize)
                        return uint.MaxValue;

                    if (pattern[j].Type == BytePattern.ValueType.Exact && MemoryDump[i + j] != pattern[j].Value)
                    {
                        cont = true;
                        break;
                    }
                }

                if (!cont)
                    return i + BaseAddress;
            }

            return uint.MaxValue;
        }
    }
}
