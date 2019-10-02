using DirtyMagic.WinAPI.Structures.Input;

namespace DirtyMagic.Hooks.Events
{
    public class MouseScrollEvent : MouseEvent
    {
        public ScrollDirection Direction { get; }
        public int Delta { get; }

        public MouseScrollEvent(WM Event, MSLLHOOKSTRUCT Raw) : base(MouseEventType.Scroll)
        {
            Delta = Raw.mouseData >> 16;
            switch (Event)
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
