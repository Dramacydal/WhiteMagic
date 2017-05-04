using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WhiteMagic.WinAPI.Structures.Hooks;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Hooks
{
    public class MouseEventInfo
    {
        private WM WMEvent;
        private MSLLHOOKSTRUCT Raw;

        public MouseEventInfo(WM Event, MSLLHOOKSTRUCT Raw)
        {
            this.Raw = Raw;
            this.WMEvent = Event;
        }

        public MouseEvent Event
        {
            get
            {
                switch (WMEvent)
                {
                    case WM.MOUSEMOVE:
                        return MouseEvent.Move;
                    case WM.LBUTTONDOWN:
                    case WM.LBUTTONUP:
                    case WM.RBUTTONDOWN:
                    case WM.RBUTTONUP:
                    case WM.MBUTTONDOWN:
                    case WM.MBUTTONUP:
                    case WM.XBUTTONDOWN:
                    case WM.XBUTTONUP:
                        return MouseEvent.Button;
                    case WM.MOUSEWHEEL:
                    case WM.MOUSEHWHEEL:
                        return MouseEvent.Wheel;
                }

                return MouseEvent.None;
            }
        }

        public int X
        {
            get
            {
                return Event == MouseEvent.Move ? Raw.ptX : -1;
            }
        }
        public int Y
        {
            get
            {
                return Event == MouseEvent.Move ? Raw.ptY : -1;
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

            public bool ButtonUp { get { return Up; } }
            public bool ButtonDown { get { return !Up; } }
        }

        public ClickInfo Click
        {
            get
            {
                if (Event != MouseEvent.Button)
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
                if (Event != MouseEvent.Wheel)
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
                case MouseEvent.Move:
                    result += $", X: {X}, Y: {Y}";
                    break;
                case MouseEvent.Button:
                    result += $", Button: {Click.Button}";
                    if (Click.ButtonDown)
                        result += ", Pressed";
                    else
                        result += ", Released";
                    break;
                case MouseEvent.Wheel:
                    result += $", Direction: {ScrollDirection}";
                    break;
            }

            return result;
        }
    }

    public delegate bool MouseMessageHandler(MouseEventInfo Info);

    public class Mouse : HookBase<MouseMessageHandler>
    {
        public Mouse() : base(HookType.WH_MOUSE_LL)
        {
        }

        public override bool Dispatch(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code != 0)
                return true;

            var Event = (WM)wParam.ToInt32();

            var raw = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            var EventInfo = new MouseEventInfo(Event, raw);

            foreach (var Handler in Handlers)
                if (!Handler(EventInfo))
                    return false;

            return true;
        }
    }
}
