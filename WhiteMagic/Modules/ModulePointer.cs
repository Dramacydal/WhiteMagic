
namespace WhiteMagic.Modules
{
    public class ModulePointer
    {
        public string ModuleName { get { return moduleName; } }
        public int Offset { get { return offset; } }

        protected string moduleName = string.Empty;
        protected int offset = 0;

        public ModulePointer(string moduleName, int offset)
        {
            this.moduleName = moduleName;
            this.offset = offset;
        }

        public static ModulePointer operator +(ModulePointer pointer, int modOffs)
        {
            return new ModulePointer(pointer.ModuleName, pointer.Offset + modOffs);
        }

        public static ModulePointer operator +(ModulePointer pointer, uint modOffs)
        {
            return new ModulePointer(pointer.ModuleName, pointer.Offset + (int)modOffs);
        }
    }
}
