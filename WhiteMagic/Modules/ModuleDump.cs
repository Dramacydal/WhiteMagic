using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WhiteMagic.Modules
{
    public class ModuleDump : MemoryContainer
    {
        protected ProcessModule Module { get; private set; }

        public string ModuleName { get { return Module.FileName; } }

        private static readonly int readCount = 256;

        public ModuleDump(ProcessModule module, MemoryHandler m)
            : base(module.BaseAddress)
        {
            this.Module = module;
            var bytes = new List<byte>();
            for (var i = 0; i < module.ModuleMemorySize; i += readCount)
                bytes.AddRange(m.ReadBytes(IntPtr.Add(module.BaseAddress, i), i + readCount >= module.ModuleMemorySize ? module.ModuleMemorySize - i : readCount));

            Data = bytes.ToArray();
        }
    }
}
