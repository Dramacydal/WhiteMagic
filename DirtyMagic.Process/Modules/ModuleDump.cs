namespace DirtyMagic.Modules
{
    public class ModuleDump : MemoryDump
    {
        protected ModuleInfo Module { get; }

        public ModuleDump(ModuleInfo module)
            : base(module.BaseAddress, module.MemorySize)
        {
            Module = module;
        }

        public bool IsInitialized  => Data != null && Data.Length > 0;
    }
}
