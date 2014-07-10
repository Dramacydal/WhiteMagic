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

            if (!WinApi.SetDebugPrivileges())
            {
                Console.WriteLine("Failed to set debug privileges");
                return;
            }

            try
            {
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

                foreach (ProcessModule module in process.Modules)
                    Console.WriteLine("{0} - 0x{1:X}", module.ModuleName, (uint)module.BaseAddress);

                pd = new ProcessDebugger(process.Id);
                Thread th = ProcessDebugger.Run(ref pd);
                if (!pd.WaitForComeUp(500))
                {
                    Console.WriteLine("Failed to start thread");
                    return;
                }

                var bp = new IncBreakPoint(0x4012B0 - 0x400000, 1, HardwareBreakPoint.Condition.Code);
                pd.AddBreakPoint("program.exe", bp);

                //pd.LoadModule("msscp.dll");

                th.Join();
            }
            catch (BreakPointException e)
            {
                Console.WriteLine("Breakpoint Exception: \"{0}\"", e.Message);
            }
            catch (DebuggerException e)
            {
                Console.WriteLine("Debugger Exception: \"{0}\"", e.Message);
            }
        }
    }
}
