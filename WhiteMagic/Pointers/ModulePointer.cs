using System;

namespace WhiteMagic.Pointers
{
    public class ModulePointer
    {
        public string ModuleName { get; protected set; }
        public IntPtr Offset { get; protected set; }

        public ModulePointer(string ModuleName, IntPtr Offset)
        {
            this.ModuleName = ModuleName;
            this.Offset = Offset;
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
