namespace DirtyMagic.Pointers
{
    public class ModulePointer
    {
        public string ModuleName { get; }
        public int Offset { get; }

        /// <summary>
        /// Pointer to named module
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="offset"></param>
        public ModulePointer(string moduleName, int offset)
        {
            this.ModuleName = moduleName;
            this.Offset = offset;
        }

        /// <summary>
        /// Pointer to main module
        /// </summary>
        /// <param name="offset"></param>
        public ModulePointer(int offset)
            : this(string.Empty, offset)
        { }

        public static ModulePointer operator +(ModulePointer pointer, int modOffs)
            => new ModulePointer(pointer.ModuleName, pointer.Offset + modOffs);

        public static ModulePointer operator +(ModulePointer pointer, uint modOffs)
            => new ModulePointer(pointer.ModuleName, pointer.Offset + (int)modOffs);
    }
}
