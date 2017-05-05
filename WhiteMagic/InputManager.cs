using System;
using WhiteMagic.Input;

namespace WhiteMagic
{
    public static class InputManager
    {
        public static class Global
        {
            public static GlobalKeyboardInput Keyboard { get; } = new GlobalKeyboardInput();

            public static GlobalMouseInput Mouse { get; } = new GlobalMouseInput();
        }

        public static WindowKeyboardInput CreateWindowInput(IntPtr Handle, bool Recursive = true)
            => new WindowKeyboardInput(Handle, Recursive);
    }
}
