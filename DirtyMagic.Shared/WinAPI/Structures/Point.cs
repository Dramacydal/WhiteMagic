using System.Runtime.InteropServices;

namespace DirtyMagic.WinAPI.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString() => $"X: {X} Y: {Y}";
    }
}
