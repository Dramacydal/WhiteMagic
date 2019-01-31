using System;
using WhiteMagic.WinAPI.Structures;

namespace WhiteMagic.Hooks
{
    public abstract class HookBase<T> where T : HookEvent
    {
        private HookType Type;

        internal HookBase(HookType Type)
        {
            this.Type = Type;
        }

        internal abstract bool Dispatch(int code, IntPtr wParam, IntPtr lParam);

        public bool IsInstalled => HookManager.IsHookInstalled(Type);

        public void Install()
        {
            HookManager.InstallHook(Type);
        }

        public void Uninstall(bool RemoveHandlers = true)
        {
            HookManager.Uninstall(Type);
            if (RemoveHandlers)
                this.RemoveHandlers();
        }

        protected void Dispatch(T e)
        {
            Handlers(e);
        }

        public event Action<T> Handlers;

        private void RemoveHandlers()
        {
            foreach (var d in Handlers.GetInvocationList())
                Handlers -= (Action<T>)d;
        }
    }
}
