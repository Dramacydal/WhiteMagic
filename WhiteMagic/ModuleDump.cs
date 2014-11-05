using System.Collections.Generic;
using System.Diagnostics;

namespace WhiteMagic
{
    public class ModuleDump
    {
        public int BaseAddress { get; set; }
        public int ModuleSize { get { return MemoryDump.Length; } }
        public byte[] MemoryDump { get; set; }

        private static readonly int readCount = 256;

        public ModuleDump(ProcessModule module, MemoryHandler m)
        {
            var bytes = new List<byte>();
            for (var i = 0; i < module.ModuleMemorySize; i += readCount)
                bytes.AddRange(m.ReadBytes((uint)(module.BaseAddress + i), i + readCount >= module.ModuleMemorySize ? module.ModuleMemorySize - i - 1 : readCount));

            BaseAddress = (int)module.BaseAddress;
            MemoryDump = bytes.ToArray();
        }

        public uint FindPattern(MemoryPattern pattern, MemoryPattern.FindOptions options = null)
        {
            if (pattern.Length == 0)
                return uint.MaxValue;

            if (options == null)
                options = new MemoryPattern.FindOptions();

            if (options.StartAddress >= BaseAddress)
                options.StartAddress -= BaseAddress;

            if (options.StartAddress >= ModuleSize)
                return uint.MaxValue;

            if (!options.Reverse)
            {
                for (var i = options.StartAddress; i < ModuleSize; ++i)
                {
                    if (i + pattern.Length > ModuleSize)
                        return uint.MaxValue;

                    var cont = false;
                    for (var j = 0; j < pattern.Length; ++j)
                    {
                        if (pattern[j].Type == MemoryPattern.ValueType.Exact && MemoryDump[i + j] != pattern[j].Value)
                        {
                            cont = true;
                            break;
                        }
                    }

                    if (!cont)
                        return (uint)(i + BaseAddress);
                }
            }
            else
            {
                if (options.StartAddress == 0)
                    options.StartAddress = (int)ModuleSize - 1;

                for (var i = options.StartAddress; i >= 0; --i)
                {
                    if (i - pattern.Length < 0)
                        return uint.MaxValue;

                    var cont = false;
                    for (var j = 0; j < pattern.Length; ++j)
                    {
                        if (pattern[j].Type == MemoryPattern.ValueType.Exact && MemoryDump[i + j - pattern.Length] != pattern[j].Value)
                        {
                            cont = true;
                            break;
                        }
                    }

                    if (!cont)
                        return (uint)(i + BaseAddress - pattern.Length);
                }
            }

            return uint.MaxValue;
        }
    }
}
