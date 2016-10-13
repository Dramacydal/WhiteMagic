using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteMagic.WinAPI.Structures;

namespace WhiteMagic
{
    public class ContextWrapper
    {
        public ProcessDebugger Debugger { get; private set; }
        public CONTEXT Context { get; private set; }

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
