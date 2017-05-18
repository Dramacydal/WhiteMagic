using System;
using System.Collections.Generic;
using WhiteMagic.WinAPI.Structures.Hooks;

namespace WhiteMagic.Hooks
{
    public abstract class HookBase<T>
    {
        private HookType Type;

        public HookBase(HookType Type)
        {
            this.Type = Type;
        }

        public abstract bool Dispatch(int code, IntPtr wParam, IntPtr lParam);

        protected List<T> Handlers = new List<T>();

        public bool Installed => Handlers.Count > 0;

        public void AttachCallback(T Handler)
        {
            HookManager.InstallHook(Type);

            Handlers.Add(Handler);
        }

        public void Remove() => Handlers.Clear();
    }
}
