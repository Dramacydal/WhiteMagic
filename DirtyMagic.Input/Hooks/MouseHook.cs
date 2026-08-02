using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DirtyMagic.Hooks.Events;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Hooks
{
    public class MouseHook() : HookBase(HookType.WH_MOUSE_LL)
    {
        public delegate void MouseClickEventHandler(MouseClickEvent e);
        public delegate void MouseMoveEventHandler(MouseMoveEvent e);
        public delegate void MouseScrollEventHandler(MouseScrollEvent e);

        public event MouseClickEventHandler OnClick;
        public event MouseMoveEventHandler OnMove;
        public event MouseScrollEventHandler OnScroll;

        private static MousePosition _lastPosition = new MousePosition(-1, -1);

        internal override bool Dispatch(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code != 0)
                return true;

            var wmEvent = (WM)wParam.ToInt32();

            try
            {
                var raw = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                MouseEvent e;

                switch (wmEvent.GetEventType())
                {
                    case MouseEventType.Button:
                        e = new MouseClickEvent(wmEvent, raw);
                        OnClick?.Invoke((MouseClickEvent) e);
                        break;
                    case MouseEventType.Move:
                        e = new MouseMoveEvent(wmEvent, raw, _lastPosition);
                        _lastPosition = ((MouseMoveEvent) e).Position;
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
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"MouseHook.Dispatch: unhandled exception from subscriber: {ex}");
            }

            return true;
        }

        public override void RemoveHandlers()
        {
            foreach (var d in OnClick?.GetInvocationList() ?? [])
                OnClick -= (MouseClickEventHandler)d;

            foreach (var d in OnMove?.GetInvocationList() ?? [])
                OnMove -= (MouseMoveEventHandler)d;

            foreach (var d in OnScroll?.GetInvocationList() ?? [])
                OnScroll -= (MouseScrollEventHandler)d;
        }
    }
}
