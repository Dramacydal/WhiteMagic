using System;
using System.Linq;

namespace DirtyMagic
{
    public static class SharedExtensions
    {
        /// <summary>
        /// Converts byte array into space-separated hex string
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Reverse"></param>
        /// <returns></returns>
        public static string AsHexString(this byte[] Data, bool Reverse = false, string Separator = " ")
        {
            if (Data.Length == 0)
                return string.Empty;

            return string.Join(Separator,
                (Reverse ? Data.Reverse() : Data).Select(_ => string.Format("{0:X2}", _)));
        }

        public static uint ToUInt32(this IntPtr Pointer) => (uint) Pointer.ToInt32();

        public static IntPtr Add(this IntPtr Pointer, int Offset) => IntPtr.Add(Pointer, Offset);

        public static IntPtr Add(this IntPtr Pointer, uint Offset) => IntPtr.Add(Pointer, (int) Offset);

        public static IntPtr Add(this IntPtr Pointer, IntPtr Pointer2) =>
            IntPtr.Add(Pointer, Pointer2.ToInt32());

        public static IntPtr Subtract(this IntPtr Pointer, int Offset) => IntPtr.Subtract(Pointer, Offset);

        public static IntPtr Subtract(this IntPtr Pointer, IntPtr Pointer2) =>
            IntPtr.Subtract(Pointer, Pointer2.ToInt32());

        public static bool IsEmpty(this TimeSpan TimeSpan) => TimeSpan.Ticks <= 0;
    }
}
