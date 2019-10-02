using System;
using System.Runtime.InteropServices;
using DirtyMagic.Hooks.Events;
using DirtyMagic.WinAPI.Structures;
using DirtyMagic.WinAPI.Structures.Input;

namespace DirtyMagic.Hooks
{
    public class MouseHook : HookBase
    {
        public MouseHook() : base(HookType.WH_MOUSE_LL)
        {
        }

        public delegate void MouseClickEventHandler(MouseClickEvent e);
        public delegate void MouseMoveEventHandler(MouseMoveEvent e);
        public delegate void MouseScrollEventHandler(MouseScrollEvent e);

        public event MouseClickEventHandler OnClick;
        public event MouseMoveEventHandler OnMove;
        public event MouseScrollEventHandler OnScroll;


        internal override bool Dispatch(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code != 0)
                return true;

            var wmEvent = (WM)wParam.ToInt32();

            var raw = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

            MouseEvent e;

            switch (wmEvent.GetEventType())
            {
                case MouseEventType.Button:
                    e = new MouseClickEvent(wmEvent, raw);
                    OnClick?.Invoke((MouseClickEvent) e);
                    break;
                case MouseEventType.Move:
                    e = new MouseMoveEvent(wmEvent, raw);
                    OnMove?.Invoke((MouseMoveEvent) e);
                    break;
                case MouseEventType.Scroll:
                    e = new MouseScrollEvent(wmEvent, raw);
                    OnScroll?.Invoke((MouseScrollEvent) e);
                    break;
                default:
                    return true;
            }

            if (e.Cancel)
                return false;

            return true;
        }

        public override void RemoveHandlers()
        {
            foreach (var d in OnClick?.GetInvocationList() ?? new Delegate[] { })
                OnClick -= (MouseClickEventHandler)d;

            foreach (var d in OnMove?.GetInvocationList() ?? new Delegate[] { })
                OnMove -= (MouseMoveEventHandler)d;

            foreach (var d in OnScroll?.GetInvocationList() ?? new Delegate[] { })
                OnScroll -= (MouseScrollEventHandler)d;
        }
    }
}
