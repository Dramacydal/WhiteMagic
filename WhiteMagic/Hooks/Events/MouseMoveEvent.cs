using System.Windows;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Hooks.Events
{
    public class MouseMoveEvent : MouseEvent
    {
        public Point Position { get; }

        public MouseMoveEvent(WM Event, MSLLHOOKSTRUCT Raw) : base(MouseEventType.Move)
        {
            Position = new Point(Raw.ptX, Raw.ptY);
        }

        public override string ToString()
        {
            return $"Event: {Type}, X: {Position.X}, Y: {Position.Y}";
        }
    }
}
