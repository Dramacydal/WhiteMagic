using System;
using System.Runtime.InteropServices;
using WhiteMagic.WinAPI.Structures.Hooks;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Hooks
{
    public class MouseInfo
    {
        private MSLLHOOKSTRUCT Raw;

        public MouseInfo(MSLLHOOKSTRUCT raw)
        {
            Raw = raw;
        }

        public int X { get { return Raw.ptX; } }
        public int Y { get { return Raw.ptY; } }

        public int XButtonIndex { get { return Raw.mouseData >> 16; } }
        public bool MouseWheelRotatedForward { get { return XButtonIndex > 0; } }
    }

    public delegate bool MouseMessageHandler(WM Event, MouseInfo Info);

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
            var Info = new MouseInfo(raw);

            foreach (var Handler in Handlers)
                if (!Handler(Event, Info))
                    return false;

            return true;
        }
    }
}
