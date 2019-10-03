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
            public static WindowKeyboardInput GetKeyboard(IntPtr Window, bool Recursive)
                => new WindowKeyboardInput(Window, Recursive);

            public static WindowMouseInput GetMouse(IntPtr Window, bool Recursive)
                => new WindowMouseInput(Window, Recursive);
        }
    }
}
