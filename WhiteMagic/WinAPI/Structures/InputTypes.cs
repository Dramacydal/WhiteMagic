using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI.Structures
{
    /// <summary>
    ///     The types used in the function <see cref="NativeMethods.SendInput" /> for input events.
    /// </summary>
    public enum InputTypes
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }
}