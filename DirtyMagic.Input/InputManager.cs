using System;
using DirtyMagic.Input;

namespace DirtyMagic
{
    public static class InputManager
    {
        public static class Global
        {
            public static GlobalKeyboardInput Keyboard { get; } = new GlobalKeyboardInput();

            public static GlobalMouseInput Mouse { get; } = new GlobalMouseInput();
        }

        public static class Window
        {
            public static WindowKeyboardInput GetKeyboard(IntPtr window, bool recursive)
                => new WindowKeyboardInput(window, recursive);

            public static WindowMouseInput GetMouse(IntPtr window, bool recursive)
                => new WindowMouseInput(window, recursive);
        }
    }
}
