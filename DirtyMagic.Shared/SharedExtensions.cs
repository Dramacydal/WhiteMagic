using System;
using System.Linq;

namespace DirtyMagic
{
    public static class SharedExtensions
    {
        /// <summary>
        /// Converts byte array into space-separated hex string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static string AsHexString(this byte[] data, bool reverse = false, string separator = " ")
        {
            if (data.Length == 0)
                return string.Empty;

            return string.Join(separator,
                (reverse ? data.Reverse() : data).Select(_ => $"{_:X2}"));
        }

        public static uint ToUInt32(this IntPtr pointer) => (uint) pointer.ToInt32();

        public static IntPtr Add(this IntPtr pointer, int offset) => IntPtr.Add(pointer, offset);

        public static IntPtr Add(this IntPtr pointer, uint offset) => IntPtr.Add(pointer, (int) offset);

        public static IntPtr Add(this IntPtr pointer, IntPtr pointer2) =>
            IntPtr.Add(pointer, pointer2.ToInt32());

        public static IntPtr Subtract(this IntPtr pointer, int offset) => IntPtr.Subtract(pointer, offset);

        public static IntPtr Subtract(this IntPtr pointer, IntPtr pointer2) =>
            IntPtr.Subtract(pointer, pointer2.ToInt32());

        public static bool IsEmpty(this TimeSpan timeSpan) => timeSpan.Ticks <= 0;
    }
}
