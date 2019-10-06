
namespace DirtyMagic.Pointers
{
    public class FunctionPointer : ModulePointer
    { 
        public MagicConvention CallingConvention { get; }

        public FunctionPointer(string moduleName, int offset, MagicConvention callingConvention)
            : base(moduleName, offset)
        {
            this.CallingConvention = callingConvention;
        }

        public void Call(MemoryHandler memory, params object[] args) => memory.Call(this, CallingConvention, args);

        public T Call<T>(MemoryHandler memory, params object[] args) where T : struct => memory.Call<T>(this, CallingConvention, args);
    }
}
