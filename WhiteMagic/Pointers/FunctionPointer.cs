
namespace WhiteMagic.Pointers
{
    public class FunctionPointer
    {
        public ModulePointer Pointer { get; private set; }
        public MagicConvention CallingConvention { get; private set; }

        public FunctionPointer(ModulePointer Pointer, MagicConvention CallingConvention)
        {
            this.Pointer = Pointer;
            this.CallingConvention = CallingConvention;
        }

        public void Call(MemoryHandler Memory, params object[] Args)
        {
            Memory.Call(Pointer, CallingConvention, Args);
        }

        public T Call<T>(MemoryHandler Memory, params object[] Args) where T : struct
        {
            return Memory.Call<T>(Pointer, CallingConvention, Args);
        }
    }
}
