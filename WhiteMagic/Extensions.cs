using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
