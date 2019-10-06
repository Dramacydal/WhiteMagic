using System;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic
{
    public class ContextWrapper
    {
        public ProcessDebugger Debugger { get; }
        public CONTEXT Context { get; }

        public ContextWrapper(ProcessDebugger debugger, CONTEXT context)
        {
            this.Debugger = debugger;
            this.Context = context;
        }

        public void Push(uint value)
        {
            Context.Esp -= 4;
            Debugger.WriteUInt(new IntPtr(Context.Esp), value);
        }

        public uint Pop()
        {
            var value = Debugger.ReadUInt(new IntPtr(Context.Esp));
            Context.Esp += 4;
            return value;
        }
    }
}
