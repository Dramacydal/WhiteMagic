using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Win32HWBP;

namespace Test
{
    class ModuleInfo
    {
        ModuleInfo(string name, uint addr)
        {
            this.name = name;
            this.addr = addr;
        }

        public string name;
        public uint addr;
    }

    class Program
    {
        private static List<ModuleInfo>  moduleInfo = new List<ModuleInfo>();

        public static uint GetModuleOffset(string name, Process p)
        {
            foreach (ProcessModule module in p.Modules)
                if (module.FileName.Split('\\').Last() == (name))
                    return (uint)module.BaseAddress;

            return 0;
        }

        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Catched Ctrl+C");
            args.Cancel = true;
            isDebugging = false;
        }

        static bool isDebugging = true;

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
            foreach (ProcessModule m in process.Modules)
                Console.WriteLine("{0} 0x{1:X}", m.FileName.Split('\\').Last(), (uint)m.BaseAddress);

            Console.WriteLine("Process Id: {0} Handle: {1} Threads: {2}", process.Id, process.Handle, process.Threads.Count);
            if (process.Threads.Count == 0)
            {
                Console.WriteLine("WTF? No threads in process");
                return;
            }

            var threadId = (uint)process.Threads[0].Id;
            Console.WriteLine("Thread Id: {0}", threadId);

            if (!WinApi.DebugActiveProcess(process.Id))
                Console.WriteLine("Failed to start debugging");
            else
                Console.WriteLine("Started debugging");

            if (!WinApi.DebugSetProcessKillOnExit(false))
                Console.WriteLine("Failed to set kill on exit");
            else
                Console.WriteLine("Set kill on exit");

            var bp = new HardwareBreakPoint((int)GetModuleOffset("program.exe", process) + 0x4012B0 - 0x400000, 1, HardwareBreakPoint.Condition.Code);
            bp.Set(threadId);
            Console.WriteLine("Address: {0:X}", GetModuleOffset("program.exe", process) + 0x4012B00 - 0x400000); 

            DEBUG_EVENT DebugEvent = new DEBUG_EVENT();
            for (; isDebugging; )
            {
                if (!WinApi.WaitForDebugEvent(ref DebugEvent, 200))
                {
                    //Console.WriteLine("Failed to wait for debug event");
                    continue;
                }

                Console.WriteLine("Debug Event Code: {0} ", DebugEvent.dwDebugEventCode);

                bool okEvent = false;
                switch (DebugEvent.dwDebugEventCode)
                {
                    case DebugEventType.RIP_EVENT:
                    case DebugEventType.EXIT_PROCESS_DEBUG_EVENT:
                        Console.WriteLine("Process has exited");
                        return;
                    case DebugEventType.EXCEPTION_DEBUG_EVENT:
                        Console.WriteLine("Exception Code: {0:X}", DebugEvent.Exception.ExceptionRecord.ExceptionCode);
                        if (DebugEvent.Exception.ExceptionRecord.ExceptionCode == (uint)ExceptonStatus.STATUS_SINGLE_STEP)
                        {
                            if (DebugEvent.dwThreadId != threadId)
                            {
                                Console.WriteLine("Debug event thread id does not match breakpoint thread");
                                break;
                            }

                            var hThread = WinApi.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, DebugEvent.dwThreadId);
                            if (hThread == IntPtr.Zero)
                                throw new BreakPointException("Failed to open thread");

                            var Context = new CONTEXT();
                            Context.ContextFlags = (uint)CONTEXT_FLAGS.CONTEXT_FULL;
                            if (!WinApi.GetThreadContext(hThread, ref Context))
                            {
                                Console.WriteLine("Failed to get thread context");
                                break;
                            }

                            if (Context.Eip != bp.Address)
                            {
                                Console.WriteLine("Exception adderss does not equal to breakpoint address");
                                return;
                            }

                            Console.WriteLine("Triggered");
                            okEvent = true;

                            // skip 'inc a'
                            Context.Eip += 6;

                            if (!WinApi.SetThreadContext(hThread, ref Context))
                                throw new BreakPointException("Failed to set thread context");
                        }
                        else
                        {
                        }
                        break;
                    default:
                        break;
                }

                if (!WinApi.ContinueDebugEvent(DebugEvent.dwProcessId, DebugEvent.dwThreadId, okEvent ? (uint)DebugContinueStatus.DBG_CONTINUE : (uint)DebugContinueStatus.DBG_EXCEPTION_NOT_HANDLED))
                {
                    Console.WriteLine("Failed to continue debug event");
                    break;
                }
            }

            bp.UnSet();

            if (!WinApi.DebugActiveProcessStop(process.Id))
                Console.WriteLine("Failed to stop debugging");
            else
                Console.WriteLine("Stopped debugging");
        }
    }
}
