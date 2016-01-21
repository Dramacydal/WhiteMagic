using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using WhiteMagic.Modules;
using WhiteMagic.Patterns;

namespace WhiteMagic
{
    public static class MagicExtensions
    {
        public static T Call<T>(this MemoryHandler m, ModulePointer offs, MagicConvention cv, params object[] args) where T : struct
        {
            return m.Call<T>(m.GetAddress(offs), cv, args);
        }

        public static void Call(this MemoryHandler m, ModulePointer offs, MagicConvention cv, params object[] args)
        {
            m.Call(m.GetAddress(offs), cv, args);
        }

        public static T Read<T>(this MemoryHandler m, ModulePointer offs) where T : struct
        {
            return m.Read<T>(m.GetAddress(offs));
        }

        public static IntPtr GetAddress(this MemoryHandler m, ModulePointer offs)
        {
            return m.GetModuleAddress(offs.ModuleName) + offs.Offset;
        }

        /// <summary>
        /// Converts byte array into space-separated hex string
        /// </summary>
        /// <param name="array"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static string AsHexString(this byte[] array, bool reverse = false, string separator = " ")
        {
            if (array.Length == 0)
                return string.Empty;

            return string.Join(separator, (reverse ? array.Reverse() : array).Select(_ => string.Format("{0:X2}", _)));
        }

        public static uint ToUInt32(this IntPtr p)
        {
            return (uint)p.ToInt32();
        }

        public static IntPtr Add(this IntPtr pointer, int offset)
        {
            return IntPtr.Add(pointer, offset);
        }

        public static IntPtr Add(this IntPtr pointer, uint offset)
        {
            return IntPtr.Add(pointer, (int)offset);
        }

        public static IntPtr Add(this IntPtr pointer, IntPtr pointer2)
        {
            return IntPtr.Add(pointer, pointer2.ToInt32());
        }

        public static IntPtr Subtract(this IntPtr pointer, int offset)
        {
            return IntPtr.Subtract(pointer, offset);
        }

        public static IntPtr Subtract(this IntPtr pointer, IntPtr pointer2)
        {
            return IntPtr.Subtract(pointer, pointer2.ToInt32());
        }

        public static string GetVersionInfo(this Process process)
        {
            return string.Format("{0} {1}.{2}.{3} {4}",
                    process.MainModule.FileVersionInfo.FileDescription,
                    process.MainModule.FileVersionInfo.FileMajorPart,
                    process.MainModule.FileVersionInfo.FileMinorPart,
                    process.MainModule.FileVersionInfo.FileBuildPart,
                    process.MainModule.FileVersionInfo.FilePrivatePart);
        }

        public static Match Match(this IEnumerable<byte> Data, MemoryPattern Pattern)
        {
            return Pattern.Match(PatternHelper.BytesToString(Data.ToArray()));
        }

        public static MatchCollection Matches(this IEnumerable<byte> Data, MemoryPattern Pattern)
        {
            return Pattern.Matches(PatternHelper.BytesToString(Data.ToArray()));
        }
    }
}
