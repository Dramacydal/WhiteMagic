namespace DirtyMagic.Hooks.Events
{
    public struct MousePosition(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
    }
}
