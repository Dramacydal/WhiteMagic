using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WhiteMagic.WinAPI.Structures;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Hooks
{
    public class MouseEvent : HookEvent
    {
        private WM WMEvent;
        private MSLLHOOKSTRUCT Raw;

        public MouseEvent(WM Event, MSLLHOOKSTRUCT Raw)
        {
            this.Raw = Raw;
            this.WMEvent = Event;
        }

        public MouseEventType Event
        {
            get
            {
                switch (WMEvent)
                {
                    case WM.MOUSEMOVE:
                        return MouseEventType.Move;
                    case WM.LBUTTONDOWN:
                    case WM.LBUTTONUP:
                    case WM.RBUTTONDOWN:
                    case WM.RBUTTONUP:
                    case WM.MBUTTONDOWN:
                    case WM.MBUTTONUP:
                    case WM.XBUTTONDOWN:
                    case WM.XBUTTONUP:
                        return MouseEventType.Button;
                    case WM.MOUSEWHEEL:
                    case WM.MOUSEHWHEEL:
                        return MouseEventType.Wheel;
                }

                return MouseEventType.None;
            }
        }

        public int X
        {
            get
            {
                if (Event == MouseEventType.Move || Event == MouseEventType.Button)
                    return Raw.ptX;
                else return -1;
            }
        }

        public int Y
        {
            get
            {
                if (Event == MouseEventType.Move || Event == MouseEventType.Button)
                    return Raw.ptY;
                else return -1;
            }
        }

        public struct ClickInfo
        {
            public ClickInfo(MouseButtons Button, bool Up)
            {
                this.Button = Button;
                this.Up = Up;
            }

            public MouseButtons Button;
            private bool Up;

            public bool ButtonUp => Up;
            public bool ButtonDown => !Up;
        }

        public ClickInfo Click
        {
            get
            {
                if (Event != MouseEventType.Button)
                    return new ClickInfo(MouseButtons.None, false);

                switch (WMEvent)
                {
                    case WM.LBUTTONDOWN:
                    case WM.LBUTTONUP:
                        return new ClickInfo(MouseButtons.Left, WMEvent == WM.LBUTTONUP);
                    case WM.RBUTTONDOWN:
                    case WM.RBUTTONUP:
                        return new ClickInfo(MouseButtons.Right, WMEvent == WM.RBUTTONUP);
                    case WM.MBUTTONDOWN:
                    case WM.MBUTTONUP:
                        return new ClickInfo(MouseButtons.Middle, WMEvent == WM.MBUTTONUP);
                    case WM.XBUTTONDOWN:
                    case WM.XBUTTONUP:
                    {
                        var xButtonIndex = Raw.mouseData >> 16;
                        if (xButtonIndex == 1)
                            return new ClickInfo(MouseButtons.XButton1, WMEvent == WM.XBUTTONUP);
                        else if (xButtonIndex == 2)
                            return new ClickInfo(MouseButtons.XButton2, WMEvent == WM.XBUTTONUP);
                            break;
                    }
                }

                return new ClickInfo(MouseButtons.None, false);
            }
        }

        public ScrollDirection ScrollDirection
        {
            get
            {
                if (Event != MouseEventType.Wheel)
                    return ScrollDirection.None;

                var delta = Raw.mouseData >> 16;
                switch (WMEvent)
                {
                    case WM.MOUSEWHEEL:
                        return delta > 0 ? ScrollDirection.Up : ScrollDirection.Down;
                    case WM.MOUSEHWHEEL:
                        return delta > 0 ? ScrollDirection.Right : ScrollDirection.Left;
                    default:
                        break;
                }

                return ScrollDirection.None;
            }
        }

        public override string ToString()
        {
            var result = $"Event: {Event}";
            switch (Event)
            {
                case MouseEventType.Move:
                    result += $", X: {X}, Y: {Y}";
                    break;
                case MouseEventType.Button:
                    result += $", Button: {Click.Button}";
                    if (Click.ButtonDown)
                        result += ", Pressed";
                    else
                        result += ", Released";
                    break;
                case MouseEventType.Wheel:
                    result += $", Direction: {ScrollDirection}";
                    break;
                default:
                    break;
            }

            return result;
        }
    }

    public class Mouse : HookBase<MouseEvent>
    {
        public Mouse() : base(HookType.WH_MOUSE_LL)
        {
        }

        internal override bool Dispatch(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code != 0)
                return true;

            var WMEvent = (WM)wParam.ToInt32();

            var raw = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            var Event = new MouseEvent(WMEvent, raw);

            Dispatch(Event);
            if (Event.Cancel)
                return false;

            return true;
        }
    }
}
