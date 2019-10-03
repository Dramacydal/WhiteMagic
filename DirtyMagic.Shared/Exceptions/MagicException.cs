using System;
using System.Runtime.InteropServices;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Exceptions
{
    public class MagicException : Exception
    {
        public ErrorCodes LastError { get; }
        public MagicException(string Message) : base(Message) { LastError = (ErrorCodes)Marshal.GetLastWin32Error(); }
    }
}
