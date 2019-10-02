using System.Windows;
using System.Windows.Forms;
using DirtyMagic.Input;
using DirtyMagic.WinAPI.Structures.Input;

namespace DirtyMagic.Hooks.Events
{
    public class MouseClickEvent : MouseEvent
    {
        public ClickState State { get; }
        public MouseButtons Button { get; }
        public Point Position { get; }

        public MouseClickEvent(WM Event, MSLLHOOKSTRUCT Raw) : base(MouseEventType.Button)
        {
            Position = new Point(Raw.ptX, Raw.ptY);

            switch (Event)
            {
                case WM.LBUTTONDOWN:
                case WM.LBUTTONUP:
                case WM.LBUTTONDBLCLK:
                    Button = MouseButtons.Left;
                    switch (Event)
                    {
                        case WM.LBUTTONDOWN:
                            State = ClickState.Down;
                            break;
                        case WM.LBUTTONUP:
                            State = ClickState.Up;
                            break;
                        case WM.LBUTTONDBLCLK:
                            State = ClickState.DoubleClick;
                            break;
                    }

                    break;
                case WM.RBUTTONDOWN:
                case WM.RBUTTONUP:
                case WM.RBUTTONDBLCLK:
                    Button = MouseButtons.Right;
                    switch (Event)
                    {
                        case WM.RBUTTONDOWN:
                            State = ClickState.Down;
                            break;
                        case WM.RBUTTONUP:
                            State = ClickState.Up;
                            break;
                        case WM.RBUTTONDBLCLK:
                            State = ClickState.DoubleClick;
                            break;
                    }

                    break;
                case WM.MBUTTONDOWN:
                case WM.MBUTTONUP:
                case WM.MBUTTONDBLCLK:
                    Button = MouseButtons.Middle;
                    switch (Event)
                    {
                        case WM.MBUTTONDOWN:
                            State = ClickState.Down;
                            break;
                        case WM.MBUTTONUP:
                            State = ClickState.Up;
                            break;
                        case WM.MBUTTONDBLCLK:
                            State = ClickState.DoubleClick;
                            break;
                    }

                    break;
                case WM.XBUTTONDOWN:
                case WM.XBUTTONUP:
                case WM.XBUTTONDBLCLK:
                {
                    var xButtonIndex = Raw.mouseData >> 16;
                    if (xButtonIndex == 1)
                        Button = MouseButtons.XButton1;
                    else if (xButtonIndex == 2)
                        Button = MouseButtons.XButton2;

                    switch (Event)
                    {
                        case WM.XBUTTONDOWN:
                            State = ClickState.Down;
                            break;
                        case WM.XBUTTONUP:
                            State = ClickState.Up;
                            break;
                        case WM.XBUTTONDBLCLK:
                            State = ClickState.DoubleClick;
                            break;
                    }

                    break;
                }
            }
        }

        public override string ToString()
        {
            return $"Event: {Type},  Button: {Button}, State: {State}";
        }
    }
}
