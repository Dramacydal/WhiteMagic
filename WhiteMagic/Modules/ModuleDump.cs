using System;
using System.Collections.Generic;

namespace WhiteMagic.Modules
{
    public class ModuleDump : MemoryContainer
    {
        protected ModuleInfo Module { get; private set; }

        private static readonly int readCount = 256;

        public void Read(MemoryHandler Memory)
        {
            var bytes = new List<byte>();
            for (var i = 0; i < Module.MemorySize; i += readCount)
                bytes.AddRange(Memory.ReadBytes(IntPtr.Add(Module.BaseAddress, i), i + readCount >= Module.MemorySize ? Module.MemorySize - i : readCount));

            Data = bytes.ToArray();
        }

        public ModuleDump(ModuleInfo Module)
            : base(Module.BaseAddress)
        {
            this.Module = Module;
        }

        public void Clear()
        {
            Data = null;
        }

        public bool Initialized { get { return Data != null && Data.Length > 0; } }
    }
}
