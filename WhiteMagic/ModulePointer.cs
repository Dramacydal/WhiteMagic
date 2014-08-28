namespace WhiteMagic
{
    public class ModulePointer
    {
        public string ModuleName { get { return moduleName; } }
        public uint Offset { get { return offset; } }

        protected string moduleName = "";
        protected uint offset = 0;

        public ModulePointer(string moduleName, uint offset)
        {
            this.moduleName = moduleName;
            this.offset = offset;
        }

        public static ModulePointer operator +(ModulePointer offs1, uint offs2)
        {
            return new ModulePointer(offs1.ModuleName, offs1.Offset + offs2);
        }
    }
}
