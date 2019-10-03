using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Input
{
    public class ScreenTranslator
    {
        /// <summary>
        /// Width of virtual screen in pixels
        /// </summary>
        public int VirtualScreenX { get; }
        /// <summary>
        /// Height of virtual screen in pixels
        /// </summary>
        public int VirtualScreenY { get; }

        private const ushort NormalBase = ushort.MaxValue;

        public ScreenTranslator()
        {
            VirtualScreenX = User32.GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN);
            VirtualScreenY = User32.GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN);
        }

        /// <summary>
        /// Translates absolute screen coordinates to normalized coordinates between 0 and 65535
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public POINT NormalizeVirtual(int X, int Y)
        {
            if (X < 0) X = 0;
            if (Y < 0) Y = 0;

            if (X > VirtualScreenX) X = VirtualScreenX;
            if (Y > VirtualScreenY) Y = VirtualScreenY;

            X = (int)(X * 1.0f / VirtualScreenX * NormalBase);
            Y = (int)(Y * 1.0f / VirtualScreenY * NormalBase);

            return new POINT(X, Y);
        }

        public POINT DenormalizeVirtual(int X, int Y)
        {
            if (X < 0) X = 0;
            if (Y < 0) Y = 0;

            if (X > NormalBase) X = NormalBase;
            if (Y > NormalBase) Y = NormalBase;

            X = (int)(X * 1.0f / NormalBase * VirtualScreenX);
            Y = (int)(Y * 1.0f / NormalBase * VirtualScreenY);

            return new POINT(X, Y);
        }
    }
}
