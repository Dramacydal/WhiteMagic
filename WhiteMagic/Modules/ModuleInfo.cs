using System;
using System.Diagnostics;

namespace WhiteMagic.Modules
{
    public class ModuleInfo
    {
        public ModuleInfo(ProcessModule Module)
        {
            Update(Module);
        }

        public void Update(ProcessModule Module)
        {
            Invalidated = false;

            if (BaseAddress == Module.BaseAddress && MemorySize == Module.ModuleMemorySize)
                return;

            ModuleName = Module.ModuleName;
            BaseAddress = Module.BaseAddress;
            MemorySize = Module.ModuleMemorySize;

            Dump = new ModuleDump(this);
        }

        public void Invalidate() { Invalidated = true; }

        public string ModuleName { get; private set; }
        public IntPtr BaseAddress { get; private set; }
        public int MemorySize { get; private set; }
        public ModuleDump Dump { get; private set; }
        public bool Invalidated { get; private set; }
    }
}
