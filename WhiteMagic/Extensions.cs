using System;
using WhiteMagic.Modules;

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

        public static IntPtr GetAddress(this ProcessDebugger pd, ModulePointer offs)
        {
            return pd.GetModuleAddress(offs.ModuleName) + offs.Offset;
        }

        /// <summary>
        /// Converts byte array into space-separated hex string
        /// </summary>
        /// <param name="array"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
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

        public static bool IsValid(this IntPtr p)
        {
            return p != new IntPtr(int.MaxValue);
        }
    }
}
