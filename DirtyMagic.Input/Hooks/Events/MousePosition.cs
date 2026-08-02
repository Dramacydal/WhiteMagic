namespace DirtyMagic.Hooks.Events
{
    public struct MousePosition
    {
        public MousePosition(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }
}
