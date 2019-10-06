
namespace DirtyMagic.Pointers
{
    public class ValuePointer<T> : ModulePointer where T : struct
    {
        public ValuePointer(string moduleName, int offset) : base(moduleName, offset) { }

        public T Read(MemoryHandler memory) => memory.Read<T>(this);

        public void Write(MemoryHandler memory, T malue) => memory.Write<T>(this, malue);
    }
}
