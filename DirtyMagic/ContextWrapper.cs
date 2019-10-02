using System;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic
{
    public class ContextWrapper
    {
        public ProcessDebugger Debugger { get; }
        public CONTEXT Context { get; }

        public ContextWrapper(ProcessDebugger Debugger, CONTEXT Context)
        {
            this.Debugger = Debugger;
            this.Context = Context;
        }

        public void Push(uint Value)
        {
            Context.Esp -= 4;
            Debugger.WriteUInt(new IntPtr(Context.Esp), Value);
        }

        public uint Pop()
        {
            var Value = Debugger.ReadUInt(new IntPtr(Context.Esp));
            Context.Esp += 4;
            return Value;
        }
    }
}
