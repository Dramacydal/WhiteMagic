
namespace WhiteMagic.Modules
{
    public class ModulePointer
    {
        public string ModuleName { get; protected set; }
        public long Offset { get; protected set; }

        public ModulePointer(string moduleName, long offset)
        {
            this.ModuleName = moduleName;
            this.Offset = offset;
        }

        public static ModulePointer operator +(ModulePointer pointer, long modOffs)
        {
            return new ModulePointer(pointer.ModuleName, pointer.Offset + modOffs);
        }

        public static ModulePointer operator +(ModulePointer pointer, ulong modOffs)
        {
            return new ModulePointer(pointer.ModuleName, pointer.Offset + (int)modOffs);
        }
    }
}
