using System.Windows;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Hooks.Events
{
    public class MouseMoveEvent : MouseEvent
    {
        public MousePosition Position { get; }
        public MousePosition LastPosition { get; }

        internal MouseMoveEvent(WM @event, MSLLHOOKSTRUCT raw, MousePosition previousPosition) : base(MouseEventType.Move)
        {
            Position = new MousePosition(raw.ptX, raw.ptY);
            LastPosition = previousPosition;
        }

        public override string ToString()
        {
            return $"Event: {Type}, X: {Position.X}, Y: {Position.Y}, LastX: {LastPosition.X}, LastY: {LastPosition.Y}";
        }
    }
}
