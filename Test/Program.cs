using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Magic;
using Win32HWBP;
using CONTEXT = Win32HWBP.CONTEXT;

namespace Test
{
    class Program
    {
        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Catched Ctrl+C");
            args.Cancel = true;
            pd.StopDebugging();
        }

        public class IncBreakPoint : HardwareBreakPoint
        {
            public IncBreakPoint(int address, uint len, Condition condition) : base(address, len, condition) { }

            public override bool HandleException(ref CONTEXT ctx, ProcessDebugger pd)
            {
                // skip 'inc a'
                ctx.Eip += 6;

                uint addr = 0x404430 - 0x400000 + pd.GetModuleAddress("program.exe");
                pd.BlackMagic.WriteUInt(addr,
                    pd.BlackMagic.ReadUInt(addr) + 10);

                return true;
            }
        }

        public static ProcessDebugger pd;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

            WinApi.SetDebugPrivileges();

            var processes = Process.GetProcessesByName("program");
            if (processes.Length == 0)
            {
                Console.WriteLine("Could not get process");
                return;
            }
            else
                Console.WriteLine("Process found");

            var process = processes.First();
            Console.WriteLine("Process Id: {0} Handle: {1} Threads: {2}", process.Id, process.Handle, process.Threads.Count);
            if (process.Threads.Count == 0)
            {
                Console.WriteLine("WTF? No threads in process");
                return;
            }

            pd = new ProcessDebugger(process.Id);
            pd.FindAndAttach();

            var bp = new IncBreakPoint(0x4012B0 - 0x400000, 1, HardwareBreakPoint.Condition.Code);
            pd.AddBreakPoint("program.exe", bp);

            pd.StartListener();

            pd.Detach();
        }
    }
}
