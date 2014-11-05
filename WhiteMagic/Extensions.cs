using System.Text;

namespace WhiteMagic
{
    public static class Extensions
    {
        public static uint Call(this ProcessDebugger pd, ModulePointer offs, CallingConventionEx cv, params object[] args)
        {
            return pd.Call(pd.GetAddress(offs), cv, args);
        }

        public static T Read<T>(this ProcessDebugger pd, ModulePointer offs)
        {
            return pd.Read<T>(pd.GetAddress(offs));
        }

        public static uint GetAddress(this ProcessDebugger pd, ModulePointer offs)
        {
            return pd.GetModuleAddress(offs.ModuleName) + offs.Offset;
        }

        public static string AsHexString(this byte[] array, bool reverse = false)
        {
            var ret = string.Empty;
            if (array.Length == 0)
                return ret;

            if (reverse)
                for (var i = array.Length - 1; i >= 0; --i)
                    ret += string.Format("{0:X} ", array[i]);
            else
                for (var i = 0; i < array.Length; ++i)
                    ret += string.Format("{0:X} ", array[i]);

            return ret;
        }
    }
}
