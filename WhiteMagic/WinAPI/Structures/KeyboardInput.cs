using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WhiteMagic.WinAPI.Structures
{
    /// <summary>
    ///     Contains information about a simulated keyboard event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        /// <summary>
        ///     A virtual-key code. The code must be a value in the range 1 to 254. If the <see cref="Flags" /> member specifies
        ///     KEYEVENTF_UNICODE, wVk must be 0.
        /// </summary>
        public Keys VirtualKey;

        /// <summary>
        ///     A hardware scan code for the key.
        ///     If <see cref="Flags" /> specifies KEYEVENTF_UNICODE, wScan specifies a Unicode character which is to be sent to the
        ///     foreground application.
        /// </summary>
        public short ScanCode;

        /// <summary>
        ///     Specifies various aspects of a keystroke.
        /// </summary>
        public KeyboardFlags Flags;

        /// <summary>
        ///     The time stamp for the event, in milliseconds. If this parameter is zero, the system will provide its own time
        ///     stamp.
        /// </summary>
        public int Time;

        /// <summary>
        ///     An additional value associated with the keystroke. Use the GetMessageExtraInfo function to obtain this information.
        /// </summary>
        public IntPtr ExtraInfo;
    }
}