using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Input
{
    public class GlobalMouseInput : IMouseInput
    {
        public override void Move(int X, int Y, bool Absolute = true)
        {
            var inp = new INPUT();
            inp.Type = InputType.MOUSE;

            inp.Union.mi.dwFlags = MouseEventFlag.MOVE;
            if (Absolute)
            {
                var Translator = new ScreenTranslator();
                var Point = Translator.NormalizeVirtual(X, Y);

                inp.Union.mi.dx = Point.X;
                inp.Union.mi.dy = Point.Y;
                inp.Union.mi.dwFlags |= MouseEventFlag.ABSOLUTE | MouseEventFlag.VIRTUALDESK;
            }
            else
            {
                inp.Union.mi.dx = X;
                inp.Union.mi.dy = Y;
            }

            if (User32.SendInput(1, ref inp, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        public override void SendButton(MouseButtons Button, bool Up = false)
        {
            var inp = new INPUT();
            inp.Type = InputType.MOUSE;

            switch (Button)
            {
                case MouseButtons.Left:
                    inp.Union.mi.dwFlags = Up ? MouseEventFlag.LEFTUP : MouseEventFlag.LEFTDOWN;
                    break;
                case MouseButtons.Right:
                    inp.Union.mi.dwFlags = Up ? MouseEventFlag.RIGHTUP : MouseEventFlag.RIGHTDOWN;
                    break;
                case MouseButtons.Middle:
                    inp.Union.mi.dwFlags = Up ? MouseEventFlag.MIDDLEUP : MouseEventFlag.MIDDLEDOWN;
                    break;
                default:
                    throw new Win32Exception($"Unsupported mouse button {Button}");
            }

            if (User32.SendInput(1, ref inp, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        private const int WHEEL_DELTA = 120;

        public override void SendScroll(ScrollDirection Direction)
        {
            var inp = new INPUT();
            inp.Type = InputType.MOUSE;

            switch (Direction)
            {
                case ScrollDirection.Up:
                case ScrollDirection.Down:
                    inp.Union.mi.dwFlags = MouseEventFlag.WHEEL;
                    inp.Union.mi.mouseData = Direction == ScrollDirection.Up ? WHEEL_DELTA : -WHEEL_DELTA;
                    break;
                case ScrollDirection.Left:
                case ScrollDirection.Right:
                    inp.Union.mi.dwFlags = MouseEventFlag.HWHEEL;
                    inp.Union.mi.mouseData = Direction == ScrollDirection.Right ? WHEEL_DELTA : -WHEEL_DELTA;
                    break;
                default:
                    throw new Win32Exception($"Unsupported scroll direction type '{Direction}'");
            }

            if (User32.SendInput(1, ref inp, INPUT.Size) != 1)
                throw new Win32Exception();
        }
    }
}
