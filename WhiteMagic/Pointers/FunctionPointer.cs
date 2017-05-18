
namespace WhiteMagic.Pointers
{
    public class FunctionPointer : ModulePointer
    { 
        public MagicConvention CallingConvention { get; }

        public FunctionPointer(string ModuleName, int Offset, MagicConvention CallingConvention)
            : base(ModuleName, Offset)
        {
            this.CallingConvention = CallingConvention;
        }

        public void Call(MemoryHandler Memory, params object[] Args) => Memory.Call(this, CallingConvention, Args);

        public T Call<T>(MemoryHandler Memory, params object[] Args) where T : struct => Memory.Call<T>(this, CallingConvention, Args);
    }
}
