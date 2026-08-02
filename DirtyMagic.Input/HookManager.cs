using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using DirtyMagic.Hooks;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic
{
    public static class HookManager
    {
        public static KeyboardHook KeyboardHookHandler { get; } = new();
        public static MouseHook MouseHookHandler { get; } = new();

        private static readonly object HookContainerLock = new();

        // store delegates to prevent their garbage collection
        private static Dictionary<HookType, User32.HookProc> Delegates { get; } = new();

        private static User32.HookProc GetHookDelegate(HookType type)
        {
            if (!Delegates.ContainsKey(type))
                Delegates[type] = (code, wParam, lParam) => GlobalHookCallback(type, code, wParam, lParam);

            return Delegates[type];
        }

        private static int GlobalHookCallback(HookType type, int code, IntPtr wParam, IntPtr lParam)
        {
            switch (type)
            {
                case HookType.WH_KEYBOARD_LL:
                {
                    if (!KeyboardHookHandler.Dispatch(code, wParam, lParam))
                        return 1;
                    break;
                }
                case HookType.WH_MOUSE_LL:
                {
                    if (!MouseHookHandler.Dispatch(code, wParam, lParam))
                        return 1;
                    break;
                }
            }

            return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        private static readonly Dictionary<HookType, IntPtr> HooksHandlesByType = new();

        internal static bool IsHookInstalled(HookType type)
        {
            lock (HookContainerLock)
            {
                return HooksHandlesByType.ContainsKey(type);
            }
        }

        internal static void InstallHook(HookType type)
        {
            lock (HookContainerLock)
            {
                if (HooksHandlesByType.ContainsKey(type))
                    return;

                using (var process = Process.GetCurrentProcess())
                using (var currentModule = process.MainModule)
                {
                    if (currentModule == null)
                        return;

                    var handle = User32.SetWindowsHookEx(type,
                        GetHookDelegate(type),
                        Kernel32.GetModuleHandle(currentModule.ModuleName),
                        0);
                    if (handle == IntPtr.Zero)
                    {
                        Delegates.Remove(type);
                        throw new Win32Exception();
                    }

                    HooksHandlesByType[type] = handle;
                }
            }
        }

        internal static void Uninstall(HookType type)
        {
            lock (HookContainerLock)
            {
                if (!HooksHandlesByType.TryGetValue(type, out var handle))
                    return;

                User32.UnhookWindowsHookEx(handle);
                HooksHandlesByType.Remove(type);
                Delegates.Remove(type);
            }
        }
    }
}
