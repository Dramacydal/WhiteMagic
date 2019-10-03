using DirtyMagic.Hooks;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic
{
    public static class InputExtensions
    {
        public static MouseEventType GetEventType(this WM message)
        {
            switch (message)
            {
                case WM.MOUSEMOVE:
                    return MouseEventType.Move;
                case WM.LBUTTONDOWN:
                case WM.LBUTTONUP:
                case WM.LBUTTONDBLCLK:
                case WM.RBUTTONDOWN:
                case WM.RBUTTONUP:
                case WM.RBUTTONDBLCLK:
                case WM.MBUTTONDOWN:
                case WM.MBUTTONUP:
                case WM.MBUTTONDBLCLK:
                case WM.XBUTTONDOWN:
                case WM.XBUTTONUP:
                case WM.XBUTTONDBLCLK:
                    return MouseEventType.Button;
                case WM.MOUSEWHEEL:
                case WM.MOUSEHWHEEL:
                    return MouseEventType.Scroll;
            }

            return MouseEventType.None;
        }
    }
}
