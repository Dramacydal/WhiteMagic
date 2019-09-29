using System;
using System.Collections.Generic;
using System.Diagnostics;
using WhiteMagic.Hooks;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures;

namespace WhiteMagic
{
    public static class HookManager
    {
        public static KeyboardHook KeyboardHookHandler { get; } = new KeyboardHook();
        public static MouseHook MouseHookHandler { get; } = new MouseHook();

        private const string HookContainerLock = "HookContainerLock";

        private static Dictionary<HookType, User32.HookProc> Delegates { get; } = new Dictionary<HookType, User32.HookProc>();
        private static User32.HookProc GetHookDelegate(HookType Type)
        {
            if (!Delegates.ContainsKey(Type))
            {
                Delegates[Type] = (int code, IntPtr wParam, IntPtr lParam) => GlobalHookCallback(Type, code, wParam, lParam);
            }

            return Delegates[Type];
        }

        private static int GlobalHookCallback(HookType Type, int code, IntPtr wParam, IntPtr lParam)
        {
            switch (Type)
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

        private static readonly Dictionary<HookType, IntPtr> HooksHandlesByType = new Dictionary<HookType, IntPtr>();

        internal static bool IsHookInstalled(HookType Type)
        {
            lock (HookContainerLock)
            {
                return HooksHandlesByType.ContainsKey(Type);
            }
        }

        internal static void InstallHook(HookType Type)
        {
            if (IsHookInstalled(Type))
                return;

            lock (HookContainerLock)
            {
                using (var process = Process.GetCurrentProcess())
                using (var currentModule = process.MainModule)
                {
                    HooksHandlesByType[Type] = User32.SetWindowsHookEx(Type,
                        GetHookDelegate(Type),
                        Kernel32.GetModuleHandle(currentModule.ModuleName),
                        0);
                }
            }
        }

        internal static void Uninstall(HookType Type)
        {
            if (!IsHookInstalled(Type))
                return;

            lock (HookContainerLock)
            {
                User32.UnhookWindowsHookEx(HooksHandlesByType[Type]);
                HooksHandlesByType.Remove(Type);
            }
        }
    }
}
