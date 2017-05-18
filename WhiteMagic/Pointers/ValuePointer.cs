
namespace WhiteMagic.Pointers
{
    public class ValuePointer<T> : ModulePointer where T : struct
    {
        public ValuePointer(string ModuleName, int Offset) : base(ModuleName, Offset) { }

        public T Read(MemoryHandler Memory) => Memory.Read<T>(this);

        public void Write(MemoryHandler Memory, T Value) => Memory.Write<T>(this, Value);
    }
}
