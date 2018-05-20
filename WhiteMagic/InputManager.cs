using System;
using WhiteMagic.Input;
using WhiteMagic.Processes;

namespace WhiteMagic
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
            public static WindowKeyboardInput GetKeyboard(RemoteWindow Window, bool Recursive)
                => Window.KeyboardInput;

            public static WindowMouseInput GetMouse(RemoteWindow Window, bool Recursive)
                => Window.MouseInput;
        }
    }
}
