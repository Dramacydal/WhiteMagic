using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Hooks.Events
{
    public class MouseScrollEvent : MouseEvent
    {
        public ScrollDirection Direction { get; }
        public int Delta { get; }

        internal MouseScrollEvent(WM @event, MSLLHOOKSTRUCT raw) : base(MouseEventType.Scroll)
        {
            Delta = raw.mouseData >> 16;
            switch (@event)
            {
                case WM.MOUSEWHEEL:
                    Direction = Delta > 0 ? ScrollDirection.Up : ScrollDirection.Down;
                    break;
                case WM.MOUSEHWHEEL:
                    Direction = Delta > 0 ? ScrollDirection.Right : ScrollDirection.Left;
                    break;
            }
        }

        public override string ToString()
        {
            return $"Event: {Type}, Direction: {Direction}, Delta: {Delta}";
        }
    }
}
