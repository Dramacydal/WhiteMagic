using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI.Structures
{
    /// <summary>
    ///     Used by <see cref="NativeMethods.SendInput" /> to store information for synthesizing input events such as
    ///     keystrokes, mouse movement, and mouse clicks.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Input
    {
        /// <summary>
        ///     Constructor that specifies a type.
        /// </summary>
        /// <param name="type">The type if the input event.</param>
        public Input(InputTypes type) : this()
        {
            Type = type;
        }

        /// <summary>
        ///     The type of the input event.
        /// </summary>
        [FieldOffset(0)]
        public InputTypes Type;

        /// <summary>
        ///     The information about a simulated mouse event.
        /// </summary>
        [FieldOffset(sizeof(int))]
        public MouseInput Mouse;

        /// <summary>
        ///     The information about a simulated keyboard event.
        /// </summary>
        [FieldOffset(sizeof(int))]
        public KeyboardInput Keyboard;

        /// <summary>
        ///     The information about a simulated hardware event.
        /// </summary>
        [FieldOffset(sizeof(int))]
        public HardwareInput Hardware;
    }
}