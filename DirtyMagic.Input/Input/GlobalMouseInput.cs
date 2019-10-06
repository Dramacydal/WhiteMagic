using System.ComponentModel;
using System.Windows.Forms;
using DirtyMagic.Hooks.Events;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Input
{
    public class GlobalMouseInput : IMouseInput
    {
        public override void Move(int x, int y, bool absolute = true)
        {
            var inp = new INPUT {Type = InputType.MOUSE};

            inp.Union.mi.dwFlags = MouseEventFlag.MOVE;
            if (absolute)
            {
                var translator = new ScreenTranslator();
                var point = translator.NormalizeVirtual(x, y);

                inp.Union.mi.dx = point.X;
                inp.Union.mi.dy = point.Y;
                inp.Union.mi.dwFlags |= MouseEventFlag.ABSOLUTE | MouseEventFlag.VIRTUALDESK;
            }
            else
            {
                inp.Union.mi.dx = x;
                inp.Union.mi.dy = y;
            }

            if (User32.SendInput(1, new[] { inp }, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        public override void SendButton(MouseButtons button, bool up = false)
        {
            var inp = new INPUT {Type = InputType.MOUSE};

            switch (button)
            {
                case MouseButtons.Left:
                    inp.Union.mi.dwFlags = up ? MouseEventFlag.LEFTUP : MouseEventFlag.LEFTDOWN;
                    break;
                case MouseButtons.Right:
                    inp.Union.mi.dwFlags = up ? MouseEventFlag.RIGHTUP : MouseEventFlag.RIGHTDOWN;
                    break;
                case MouseButtons.Middle:
                    inp.Union.mi.dwFlags = up ? MouseEventFlag.MIDDLEUP : MouseEventFlag.MIDDLEDOWN;
                    break;
                default:
                    throw new Win32Exception($"Unsupported mouse button {button}");
            }

            if (User32.SendInput(1, new[] { inp }, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        private const int WHEEL_DELTA = 120;

        public override void SendScroll(ScrollDirection direction)
        {
            var inp = new INPUT {Type = InputType.MOUSE};

            switch (direction)
            {
                case ScrollDirection.Up:
                case ScrollDirection.Down:
                    inp.Union.mi.dwFlags = MouseEventFlag.WHEEL;
                    inp.Union.mi.mouseData = direction == ScrollDirection.Up ? WHEEL_DELTA : -WHEEL_DELTA;
                    break;
                case ScrollDirection.Left:
                case ScrollDirection.Right:
                    inp.Union.mi.dwFlags = MouseEventFlag.HWHEEL;
                    inp.Union.mi.mouseData = direction == ScrollDirection.Right ? WHEEL_DELTA : -WHEEL_DELTA;
                    break;
                default:
                    throw new Win32Exception($"Unsupported scroll direction type '{direction}'");
            }

            if (User32.SendInput(1, new[] { inp }, INPUT.Size) != 1)
                throw new Win32Exception();
        }
    }
}
