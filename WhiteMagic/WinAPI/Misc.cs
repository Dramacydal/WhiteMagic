using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI
{
    public partial class WinApi
    {
        [DllImport("user32.dll")]
        public static extern bool BlockInput(bool fBlockIt);
    }
}
