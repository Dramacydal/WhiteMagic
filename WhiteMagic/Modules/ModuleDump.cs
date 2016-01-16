using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using WhiteMagic.Patterns;

namespace WhiteMagic.Modules
{
    public class ModuleDump
    {
        private static readonly Encoding ConversionEncoding = Encoding.GetEncoding(28591);

        protected ProcessModule Module { get; private set; }

        public string ModuleName { get { return Module.FileName; } }
        public IntPtr BaseAddress { get { return Module.BaseAddress; } }

        public int ModuleSize { get { return Raw.Length; } }
        public byte[] Raw { get; private set; }
        protected string StringDump { get; private set; }

        private static readonly int readCount = 256;

        public ModuleDump(ProcessModule module, MemoryHandler m)
        {
            this.Module = module;
            var bytes = new List<byte>();
            for (var i = 0; i < module.ModuleMemorySize; i += readCount)
                bytes.AddRange(m.ReadBytes(IntPtr.Add(module.BaseAddress, i), i + readCount >= module.ModuleMemorySize ? module.ModuleMemorySize - i : readCount));

            Raw = bytes.ToArray();
            StringDump = ConversionEncoding.GetString(Raw, 0, Raw.Length);
        }

        public Match Match(MemoryPattern Pattern)
        {
            return Pattern.Match(StringDump);
        }

        public MatchCollection Matches(MemoryPattern Pattern)
        {
            return Pattern.Matches(StringDump);
        }
    }
}
