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
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public POINT NormalizeVirtual(int x, int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            if (x > VirtualScreenX) x = VirtualScreenX;
            if (y > VirtualScreenY) y = VirtualScreenY;

            x = (int)(x * 1.0f / VirtualScreenX * NormalBase);
            y = (int)(y * 1.0f / VirtualScreenY * NormalBase);

            return new POINT(x, y);
        }

        public POINT DenormalizeVirtual(int x, int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            if (x > NormalBase) x = NormalBase;
            if (y > NormalBase) y = NormalBase;

            x = (int)(x * 1.0f / NormalBase * VirtualScreenX);
            y = (int)(y * 1.0f / NormalBase * VirtualScreenY);

            return new POINT(x, y);
        }
    }
}
