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

        public uint FindPattern(MemoryPattern pattern, int startAddress = 0, bool reverse = false)
        {
            if (startAddress >= BaseAddress)
                startAddress -= BaseAddress;

            if (startAddress >= ModuleSize)
                return uint.MaxValue;

            if (pattern.Length == 0)
                return (uint)startAddress;

            for (var curAddr = startAddress; curAddr < ModuleSize; ++curAddr)
            {
                if (curAddr + pattern.Length > ModuleSize)
                    return uint.MaxValue;

                var match = true;
                var offs = 0;
                for (var j = 0; j < pattern.Length; ++j)
                {
                    ///
                    if (j > 500)
                        return uint.MaxValue;
                    ///
                    var e = pattern[j];
                    if (!e.Matches(MemoryDump[curAddr + offs]))
                    {
                        match = false;
                        break;
                    }

                    if (e.Type == MemoryPattern.ValueType.AnySequence)
                    {
                        if (j == pattern.Length - 1)
                            return (uint)(curAddr + BaseAddress);

                        var seqMatches = false;
                        var seqLength = 0;
                        for (; seqLength <= e.MaxLength; ++seqLength)
                        {
                            if (curAddr + 1 + seqLength >= ModuleSize)
                                return uint.MaxValue;

                            if (pattern[j + 1].Matches(MemoryDump[curAddr + 1 + seqLength]))
                            {
                                if (seqLength >= e.MinLength)
                                {
                                    seqMatches = true;
                                    break;
                                }
                            }
                        }

                        if (!seqMatches)
                        {
                            match = false;
                            break;
                        }

                        offs += seqLength;
                    }
                    else
                        ++offs;
                }

                if (match)
                    return (uint)(curAddr + BaseAddress);
            }

            return uint.MaxValue;
        }
    }
}
