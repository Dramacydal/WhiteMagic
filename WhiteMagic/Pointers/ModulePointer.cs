using System;

namespace WhiteMagic.Pointers
{
    public class ModulePointer
    {
        public string ModuleName { get; protected set; }
        public int Offset { get; protected set; }

        /// <summary>
        /// Pointer to named module
        /// </summary>
        /// <param name="ModuleName"></param>
        /// <param name="Offset"></param>
        public ModulePointer(string ModuleName, int Offset)
        {
            this.ModuleName = ModuleName;
            this.Offset = Offset;
        }

        /// <summary>
        /// Pointer to main module
        /// </summary>
        /// <param name="Offset"></param>
        public ModulePointer(int Offset)
            : this(string.Empty, Offset)
        { }

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
