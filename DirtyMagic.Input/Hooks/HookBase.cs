using System;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Hooks
{
    public abstract class HookBase
    {
        private readonly HookType _type;

        internal HookBase(HookType Type)
        {
            _type = Type;
        }

        internal abstract bool Dispatch(int code, IntPtr wParam, IntPtr lParam);

        public bool IsInstalled => HookManager.IsHookInstalled(_type);

        public void Install()
        {
            HookManager.InstallHook(_type);
        }

        public void Uninstall(bool removeHandlers = true)
        {
            HookManager.Uninstall(_type);
            if (removeHandlers)
                this.RemoveHandlers();
        }

        public abstract void RemoveHandlers();
    }
}
