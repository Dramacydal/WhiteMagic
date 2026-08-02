namespace DirtyMagic.Hooks.Events
{
    public abstract class MouseEvent(MouseEventType type) : HookEvent
    {
        protected MouseEventType Type { get; } = type;

        public abstract override string ToString();
    }
}
