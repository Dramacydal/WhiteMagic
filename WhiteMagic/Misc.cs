using System.Runtime.InteropServices;

namespace WhiteMagic
{
    public partial class WinApi
    {
        [DllImport("user32.dll")]
        public static extern bool BlockInput(bool fBlockIt);
    }
}
