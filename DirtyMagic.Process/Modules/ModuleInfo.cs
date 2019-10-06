using System;
using System.Diagnostics;

namespace DirtyMagic.Modules
{
    public class ModuleInfo
    {
        public ModuleInfo(ProcessModule module)
        {
            Update(module);
        }

        public void Update(ProcessModule module)
        {
            IsInvalidated = false;

            if (BaseAddress == module.BaseAddress && MemorySize == module.ModuleMemorySize)
                return;

            ModuleName = module.ModuleName;
            BaseAddress = module.BaseAddress;
            MemorySize = module.ModuleMemorySize;

            Dump = new ModuleDump(this);
        }

        public void Invalidate() => IsInvalidated = true;

        public string ModuleName { get; private set; }
        public IntPtr BaseAddress { get; private set; }
        public int MemorySize { get; private set; }
        public ModuleDump Dump { get; private set; }
        public bool IsInvalidated { get; private set; }
    }
}
