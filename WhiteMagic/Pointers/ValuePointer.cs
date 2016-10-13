
namespace WhiteMagic.Pointers
{
    public class ValuePointer<T> where T : struct
    {
        public ModulePointer Pointer { get; private set; }

        public ValuePointer(ModulePointer Pointer)
        {
            this.Pointer = Pointer;
        }

        public T Read(MemoryHandler Memory)
        {
            return Memory.Read<T>(Pointer);
        }

        public void Write(MemoryHandler Memory, T Value)
        {
            Memory.Write<T>(Pointer, Value);
        }
    }
}
