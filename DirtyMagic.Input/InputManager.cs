using System;
using DirtyMagic.Input;

namespace DirtyMagic
{
    public static class InputManager
    {
        public static class Global
        {
            public static GlobalKeyboardInput Keyboard { get; } = new();

            public static GlobalMouseInput Mouse { get; } = new();
        }

        public static class Window
        {
            public static WindowKeyboardInput GetKeyboard(IntPtr window, bool recursive) => new(window, recursive);

            public static WindowMouseInput GetMouse(IntPtr window, bool recursive) => new(window, recursive);
        }
    }
}
