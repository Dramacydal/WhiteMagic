
namespace WhiteMagic.Modules
{
    public class ModulePointer
    {
        public string ModuleName { get; protected set; }
        public int Offset { get; protected set; }

        public ModulePointer(string moduleName, int offset)
        {
            this.ModuleName = moduleName;
            this.Offset = offset;
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
