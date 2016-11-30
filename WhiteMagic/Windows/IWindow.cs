using System;
using System.Collections.Generic;
using WhiteMagic.WinAPI.Structures;

namespace WhiteMagic.Windows
{
    public interface IWindow : IDisposable
    {
        IEnumerable<IWindow> Children { get; }
        string ClassName { get; }
        IntPtr Handle { get; }
        int Height { get; set; }
        bool IsActivated { get; }
        IKeyboard Keyboard { get; set; }
        IMouse Mouse { get; set; }
        WindowPlacement Placement { get; set; }
        WindowStates State { get; set; }
        string Title { get; set; }
        int Width { get; set; }
        int X { get; set; }
        int Y { get; set; }
        void Activate();
        void Close();
        void Flash();
        void Flash(int count, TimeSpan timeout, FlashWindowFlags flags = FlashWindowFlags.All);
        void PostMessage(WM message, IntPtr wParam, IntPtr lParam);
        void PostMessage(int message, IntPtr wParam, IntPtr lParam);
        IntPtr SendMessage(WM message, IntPtr wParam, IntPtr lParam);
        IntPtr SendMessage(int message, IntPtr wParam, IntPtr lParam);
    }
}