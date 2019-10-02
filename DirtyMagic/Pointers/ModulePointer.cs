namespace DirtyMagic.Pointers
{
    public class ModulePointer
    {
        public string ModuleName { get; }
        public int Offset { get; }

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
            => new ModulePointer(pointer.ModuleName, pointer.Offset + modOffs);

        public static ModulePointer operator +(ModulePointer pointer, uint modOffs)
            => new ModulePointer(pointer.ModuleName, pointer.Offset + (int)modOffs);
    }
}
