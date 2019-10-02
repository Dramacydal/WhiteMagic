namespace DirtyMagic.Hooks.Events
{
    public abstract class MouseEvent : HookEvent
    {
        protected MouseEventType Type { get; }

        protected MouseEvent(MouseEventType type)
        {
            Type = type;
        }

        public abstract override string ToString();
    }
}
