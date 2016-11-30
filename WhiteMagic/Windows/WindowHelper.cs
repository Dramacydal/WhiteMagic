using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures;

namespace WhiteMagic.Windows
{
    public static class WindowHelper
    {   
        public static string GetClassName(IntPtr windowHandle)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");

            var stringBuilder = new StringBuilder(char.MaxValue);

            if (User32.GetClassName(windowHandle, stringBuilder, stringBuilder.Capacity) == 0)
                throw new Win32Exception("Couldn't get the class name of the window or the window has no class name.");

            return stringBuilder.ToString();
        }
 
        public static IntPtr GetForegroundWindow()
        {
            return User32.GetForegroundWindow();
        }
  
        public static int GetSystemMetrics(SystemMetrics metric)
        {
            var ret = User32.GetSystemMetrics(metric);

            if (ret != 0)
                return ret;

            throw new Win32Exception(
                "The call of GetSystemMetrics failed. Unfortunately, GetLastError code doesn't provide more information.");
        }

        public static string GetWindowText(IntPtr windowHandle)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");

            var capacity = User32.GetWindowTextLength(windowHandle);

            if (capacity == 0)
                return string.Empty;

            var stringBuilder = new StringBuilder(capacity + 1);
            if (User32.GetWindowText(windowHandle, stringBuilder, stringBuilder.Capacity) == 0)
                throw new Win32Exception("Couldn't get the text of the window's title bar or the window has no title.");

            return stringBuilder.ToString();
        }
     
        public static WindowPlacement GetWindowPlacement(IntPtr windowHandle)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");

            WindowPlacement placement;
            placement.Length = Marshal.SizeOf(typeof(WindowPlacement));

            if (!User32.GetWindowPlacement(windowHandle, out placement))
                throw new Win32Exception("Couldn't get the window placement.");

            return placement;
        }
   
        public static int GetWindowProcessId(IntPtr windowHandle)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");

            int processId;
            User32.GetWindowThreadProcessId(windowHandle, out processId);

            return processId;
        }
      
        public static int GetWindowThreadId(IntPtr windowHandle)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");
            int trash;
            return User32.GetWindowThreadProcessId(windowHandle, out trash);
        }

        public static IEnumerable<IntPtr> EnumAllWindows()
        {
            var list = new List<IntPtr>();

            foreach (var topWindow in EnumTopLevelWindows())
            {
                list.Add(topWindow);
                list.AddRange(EnumChildWindows(topWindow));
            }
            return list;
        }

        public static IEnumerable<IntPtr> EnumChildWindows(IntPtr parentHandle)
        {
            var list = new List<IntPtr>();

            User32.EnumWindowsProc callback = delegate (IntPtr windowHandle, IntPtr lParam)
            {
                list.Add(windowHandle);
                return true;
            };

            User32.EnumChildWindows(parentHandle, callback, IntPtr.Zero);
            return list.ToArray();
        }
    
        public static IEnumerable<IntPtr> EnumTopLevelWindows()
        {
            return EnumChildWindows(IntPtr.Zero);
        }

        public static bool FlashWindow(IntPtr windowHandle)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");

            return User32.FlashWindow(windowHandle, true);
        }

        public static void FlashWindowEx(IntPtr windowHandle, FlashWindowFlags flags, int count, TimeSpan timeout)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");

            var flashInfo = new FlashInfo
            {
                Size = Marshal.SizeOf(typeof(FlashInfo)),
                Hwnd = windowHandle,
                Flags = flags,
                Count = count,
                Timeout = Convert.ToInt32(timeout.TotalMilliseconds)
            };

            User32.FlashWindowEx(ref flashInfo);
        }

        public static void FlashWindowEx(IntPtr windowHandle, FlashWindowFlags flags, int count)
        {
            FlashWindowEx(windowHandle, flags, count, TimeSpan.FromMilliseconds(0));
        }

        public static void FlashWindowEx(IntPtr windowHandle, FlashWindowFlags flags)
        {
            FlashWindowEx(windowHandle, flags, 0);
        }

        public static uint MapVirtualKey(uint key, TranslationTypes translation)
        {
            return User32.MapVirtualKey(key, translation);
        }

        public static uint MapVirtualKey(Keys key, TranslationTypes translation)
        {
            return MapVirtualKey((uint)key, translation);
        }

        public static void PostMessage(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");
            
            if (!User32.PostMessage(windowHandle, message, wParam, lParam))
                throw new Win32Exception($"Couldn't post the message '{message}'.");
        }

        public static void PostMessage(IntPtr windowHandle, int message, UIntPtr wParam, UIntPtr lParam)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");

            if (!User32.PostMessage(windowHandle, message, wParam, lParam))
                throw new Win32Exception($"Couldn't post the message '{message}'.");
        }
        public static void PostMessage(IntPtr windowHandle, WM message, IntPtr wParam, IntPtr lParam)
        {
            PostMessage(windowHandle, (int)message, wParam, lParam);
        }

        public static void SendInput(Input[] inputs)
        {
            if (inputs != null && inputs.Length != 0)
            {
                if (User32.SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(Input))) == 0)
                    throw new Win32Exception("Couldn't send the inputs.");
            }
            else
                throw new ArgumentException("The parameter cannot be null or empty.", nameof(inputs));
        }

        public static void SendInput(Input input)
        {
            SendInput(new[] { input });
        }

        public static IntPtr SendMessage(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");
            return User32.SendMessage(windowHandle, message, wParam, lParam);
        }

        public static IntPtr SendMessage(IntPtr windowHandle, WM message, IntPtr wParam, IntPtr lParam)
        {
            return SendMessage(windowHandle, (int)message, wParam, lParam);
        }
  
        public static IntPtr SendMessage(Message message)
        {
            return SendMessage(message.HWnd, message.Msg, message.WParam, message.LParam);
        }

        public static void SetForegroundWindow(IntPtr windowHandle)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");

            if (GetForegroundWindow() == windowHandle)
                return;

            ShowWindow(windowHandle, WindowStates.Restore);

            if (!User32.SetForegroundWindow(windowHandle))
                throw new ApplicationException("Couldn't set the window to foreground.");
        }

        public static void SetWindowPlacement(IntPtr windowHandle, int left, int top, int height, int width)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");
      
            var placement = GetWindowPlacement(windowHandle);

            placement.NormalPosition.Left = left;
            placement.NormalPosition.Top = top;
            placement.NormalPosition.Height = height;
            placement.NormalPosition.Width = width;

            SetWindowPlacement(windowHandle, placement);
        }
      
        public static void SetWindowPlacement(IntPtr windowHandle, WindowPlacement placement)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");
        
            if (Debugger.IsAttached && placement.ShowCmd == WindowStates.ShowNormal)
                placement.ShowCmd = WindowStates.Restore;

            if (!User32.SetWindowPlacement(windowHandle, ref placement))
                throw new Win32Exception("Couldn't set the window placement.");
        }
     
        public static void SetWindowText(IntPtr windowHandle, string title)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");
            if (!User32.SetWindowText(windowHandle, title))
                throw new Win32Exception("Couldn't set the text of the window's title bar.");
        }

        public static bool ShowWindow(IntPtr windowHandle, WindowStates state)
        {
            HandleManipulator.ValidateAsArgument(windowHandle, "windowHandle");
            return User32.ShowWindow(windowHandle, state);
        }

        public static IntPtr GetMainWindowHandle(string processName)
        {
            var firstOrDefault = Process.GetProcessesByName(processName);

            var process = firstOrDefault.FirstOrDefault();

            if (process == null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            return process.MainWindowHandle;
        }
    }
}