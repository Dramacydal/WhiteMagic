﻿using System;
using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI.Structures.Process
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }
}
