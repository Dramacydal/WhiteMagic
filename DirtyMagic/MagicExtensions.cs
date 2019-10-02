using System;
using System.Diagnostics;
using System.Linq;
using DirtyMagic.Hooks;
using DirtyMagic.Processes;
using DirtyMagic.WinAPI.Structures.Input;
using DirtyMagic.Hooks.Events;
using DirtyMagic.Modules;

namespace DirtyMagic
{
    public static class MagicExtensions
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

            return string.Join(Separator, (Reverse ? Data.Reverse() : Data).Select(_ => string.Format("{0:X2}", _)));
        }

        public static uint ToUInt32(this IntPtr Pointer) => (uint)Pointer.ToInt32();

        public static IntPtr Add(this IntPtr Pointer, int Offset) => IntPtr.Add(Pointer, Offset);

        public static IntPtr Add(this IntPtr Pointer, uint Offset) => IntPtr.Add(Pointer, (int)Offset);

        public static IntPtr Add(this IntPtr Pointer, IntPtr Pointer2) => IntPtr.Add(Pointer, Pointer2.ToInt32());

        public static IntPtr Subtract(this IntPtr Pointer, int Offset) => IntPtr.Subtract(Pointer, Offset);

        public static IntPtr Subtract(this IntPtr Pointer, IntPtr Pointer2) => IntPtr.Subtract(Pointer, Pointer2.ToInt32());

        public static string GetVersionInfo(this RemoteProcess Process)
        {
            return string.Format("{0} {1}.{2}.{3} {4}",
                    Process.MainModule.FileVersionInfo.FileDescription,
                    Process.MainModule.FileVersionInfo.FileMajorPart,
                    Process.MainModule.FileVersionInfo.FileMinorPart,
                    Process.MainModule.FileVersionInfo.FileBuildPart,
                    Process.MainModule.FileVersionInfo.FilePrivatePart);
        }

        public static bool IsEmpty(this TimeSpan TimeSpan) => TimeSpan.Ticks <= 0;

        public static MouseEventType GetEventType(this WM message)
        {
            switch (message)
            {
                case WM.MOUSEMOVE:
                    return MouseEventType.Move;
                case WM.LBUTTONDOWN:
                case WM.LBUTTONUP:
                case WM.LBUTTONDBLCLK:
                case WM.RBUTTONDOWN:
                case WM.RBUTTONUP:
                case WM.RBUTTONDBLCLK:
                case WM.MBUTTONDOWN:
                case WM.MBUTTONUP:
                case WM.MBUTTONDBLCLK:
                case WM.XBUTTONDOWN:
                case WM.XBUTTONUP:
                case WM.XBUTTONDBLCLK:
                    return MouseEventType.Button;
                case WM.MOUSEWHEEL:
                case WM.MOUSEHWHEEL:
                    return MouseEventType.Scroll;
            }

            return MouseEventType.None;
        }
    }
}
