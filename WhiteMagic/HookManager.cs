using System;
using System.Collections.Generic;
using System.Diagnostics;
using WhiteMagic.Hooks;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures.Hooks;

namespace WhiteMagic
{
    public static class HookManager
    {
        public static Keyboard KeyboardHooks { get; } = new Keyboard();
        public static Mouse MouseHooks { get; } = new Mouse();

        private const string HookContainerLock = "HookContainerLock";

        private static Dictionary<HookType, User32.HookProc> Delegates { get; } = new Dictionary<HookType, User32.HookProc>();
        private static User32.HookProc GetHookDelegate(HookType Type)
        {
            if (!Delegates.ContainsKey(Type))
            {
                Delegates[Type] = (int code, IntPtr wParam, IntPtr lParam) =>
                {
                    return GlobalHookCallback(Type, code, wParam, lParam);
                };
            }

            return Delegates[Type];
        }

        private static int GlobalHookCallback(HookType Type, int code, IntPtr wParam, IntPtr lParam)
        {
            switch (Type)
            {
                case HookType.WH_KEYBOARD_LL:
                {
                    if (!KeyboardHooks.Dispatch(code, wParam, lParam))
                        return 1;
                    break;
                }
                case HookType.WH_MOUSE_LL:
                {
                    if (!MouseHooks.Dispatch(code, wParam, lParam))
                        return 1;
                    break;
                }
                default:
                    break;
            }

            return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        private static Dictionary<HookType, IntPtr> HooksHandlesByType = new Dictionary<HookType, IntPtr>();

        public static void InstallHook(HookType Type)
        {
            lock (HookContainerLock)
            {
                if (!HooksHandlesByType.ContainsKey(Type))
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
        }

        public static void Uninstall()
        {
            lock (HookContainerLock)
            {
                KeyboardHooks.Remove();
                MouseHooks.Remove();

                foreach (var Handle in HooksHandlesByType)
                    User32.UnhookWindowsHookEx(Handle.Value);

                HooksHandlesByType.Clear();
            }
        }
    }
}
