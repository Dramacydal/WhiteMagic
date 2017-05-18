namespace WhiteMagic.Modules
{
    public class ModuleDump : MemoryDump
    {
        protected ModuleInfo Module { get; }

        public ModuleDump(ModuleInfo Module)
            : base(Module.BaseAddress, Module.MemorySize)
        {
            this.Module = Module;
        }

        public bool Initialized  => Data != null && Data.Length > 0;
    }
}
