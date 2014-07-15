﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magic;


namespace Win32HWBP
{
    using BreakPointContainer = List<HardwareBreakPoint>;

    public class DebuggerException : Exception
    {
        public DebuggerException(string message) : base(message) { }
    }

    public class ProcessDebugger
    {
        protected int processId;
        protected int threadId = 0;
        protected bool isDebugging = false;
        protected bool isDetached = false;
        protected BreakPointContainer breakPoints = new BreakPointContainer();
        protected BlackMagic bm;
        protected Process process;
        protected Thread debugThread;

        public int ProcessId { get { return processId; } }
        public int ThreadId { get { return threadId; } }
        public bool IsDebugging { get { return isDebugging; } }
        public bool IsDetached { get { return isDetached; } }
        public BreakPointContainer Breakpoints { get { return breakPoints; } }
        public BlackMagic BlackMagic { get { return bm; } }

        public ProcessDebugger(int processId)
        {
            this.processId = processId;
        }

        public void FindAndAttach()
        {
            process = Process.GetProcessById(processId);
            if (process == null)
                throw new DebuggerException("Process " + processId + " not found");

            threadId = process.Threads[0].Id;

            bool res = false;
            if (!WinApi.CheckRemoteDebuggerPresent(process.Handle, ref res))
                throw new DebuggerException("Failed to check if remote process is already being debugged");

            if (res)
                throw new DebuggerException("Process is already being debugged by another debugger");

            if (!WinApi.DebugActiveProcess(process.Id))
                throw new DebuggerException("Failed to start debugging");

            if (!WinApi.DebugSetProcessKillOnExit(false))
                throw new DebuggerException("Failed to set kill on exit");

            bm = new BlackMagic(processId);
            isDebugging = true;
        }

        protected uint GetModuleAddress(string moduleName)
        {
            foreach (ProcessModule module in process.Modules)
                if (module.ModuleName.ToLower() == moduleName.ToLower())
                    return (uint)module.BaseAddress;

            return 0;
        }

        protected void LoadModule(string name)
        {
            uint funcAddress = WinApi.GetProcAddress(GetModuleAddress("kernel32.dll"), "LoadLibraryA");
            Console.WriteLine("{0:X}", funcAddress);

            var addr = bm.AllocateMemory(1024);
            var argaddr = bm.AllocateMemory(1024);
            bm.WriteASCIIString(argaddr, name);

            bm.Asm.Clear();
            bm.Asm.AddLine("push {0}", argaddr);
            bm.Asm.AddLine("call {0}", funcAddress);
            bm.Asm.AddLine("retn");
            bm.Asm.InjectAndExecute(addr);

            bm.FreeMemory(addr);
            bm.FreeMemory(argaddr);
        }

        public void AddBreakPoint(string moduleName, HardwareBreakPoint bp)
        {
            uint moduleBase = GetModuleAddress(moduleName);
            if (moduleBase == 0)
                throw new DebuggerException("Module " + moduleName + " is not loaded");

            int offs = (int)bp.Address;
            if (offs > 0)
                bp.Shift(moduleBase);
            else
                bp.Shift(WinApi.GetProcAddressOrdinal(moduleBase, (uint)Math.Abs(offs)), true);

            try
            {
                bp.Set(threadId);
            }
            catch (BreakPointException e)
            {
                throw new DebuggerException(e.Message);
            }

            breakPoints.Add(bp);
        }

        public void RemoveBreakPoint(uint address)
        {
            HardwareBreakPoint bp = breakPoints.Find(b => b.Address == address);
            if (bp == null)
                return;

            try
            {
                bp.UnSet();
            }
            catch (BreakPointException e)
            {
                throw new DebuggerException(e.Message);
            }

            breakPoints.Remove(bp);
        }

        public void RemoveBreakPoints()
        {
            try
            {
                foreach (var bp in breakPoints)
                    bp.UnSet();

                breakPoints.Clear();
            }
            catch (BreakPointException e)
            {
                throw new DebuggerException(e.Message);
            }
        }

        public void StopDebugging()
        {
            if (!isDebugging)
                return;

            isDebugging = false;
        }

        protected void Detach()
        {
            if (isDetached)
                return;
            isDetached = true;

            RemoveBreakPoints();

            if (!WinApi.DebugActiveProcessStop(processId))
                throw new DebuggerException("Failed to stop process debugging");
        }

        public void StartListener(uint waitInterval = 200)
        {
            DEBUG_EVENT DebugEvent = new DEBUG_EVENT();
            for (; isDebugging; )
            {
                if (!WinApi.WaitForDebugEvent(ref DebugEvent, waitInterval))
                {
                    if (!isDebugging)
                        break;
                    continue;
                }

                //Console.WriteLine("Debug Event Code: {0} ", DebugEvent.dwDebugEventCode);

                bool okEvent = false;
                switch (DebugEvent.dwDebugEventCode)
                {
                    case DebugEventType.RIP_EVENT:
                    case DebugEventType.EXIT_PROCESS_DEBUG_EVENT:
                        //Console.WriteLine("Process has exited");
                        isDetached = true;
                        isDebugging = false;
                        return;
                    case DebugEventType.EXCEPTION_DEBUG_EVENT:
                        Console.WriteLine("Exception Code: {0:X}", DebugEvent.Exception.ExceptionRecord.ExceptionCode);
                        if (DebugEvent.Exception.ExceptionRecord.ExceptionCode == (uint)ExceptonStatus.STATUS_SINGLE_STEP)
                        {
                            /*if (DebugEvent.dwThreadId != threadId)
                            {
                                Console.WriteLine("Debug event thread id does not match breakpoint thread");
                                break;
                            }*/

                            var hThread = WinApi.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, DebugEvent.dwThreadId);
                            if (hThread == IntPtr.Zero)
                                throw new DebuggerException("Failed to open thread");

                            var Context = new CONTEXT();
                            Context.ContextFlags = (uint)CONTEXT_FLAGS.CONTEXT_FULL;
                            if (!WinApi.GetThreadContext(hThread, ref Context))
                                throw new DebuggerException("Failed to get thread context");

                            HardwareBreakPoint bp = breakPoints.Find(b => b.Address == Context.Eip &&
                                b.ThreadId == DebugEvent.dwThreadId);
                            if (bp == null)
                                break;

                            Console.WriteLine("Triggered");
                            okEvent = true;

                            if (bp.HandleException(ref Context, this) && !WinApi.SetThreadContext(hThread, ref Context))
                                throw new DebuggerException("Failed to set thread context");
                        }
                        break;
                    default:
                        break;
                }

                if (!isDebugging)
                {
                    isDetached = true;

                    RemoveBreakPoints();
                    if (!WinApi.ContinueDebugEvent(DebugEvent.dwProcessId, DebugEvent.dwThreadId, okEvent ? (uint)DebugContinueStatus.DBG_CONTINUE : (uint)DebugContinueStatus.DBG_EXCEPTION_NOT_HANDLED))
                        throw new DebuggerException("Failed to continue debug event");
                    if (!WinApi.DebugActiveProcessStop(processId))
                        throw new DebuggerException("Failed to stop process debugging");
                    return;
                }

                if (!WinApi.ContinueDebugEvent(DebugEvent.dwProcessId, DebugEvent.dwThreadId, okEvent ? (uint)DebugContinueStatus.DBG_CONTINUE : (uint)DebugContinueStatus.DBG_EXCEPTION_NOT_HANDLED))
                    throw new DebuggerException("Failed to continue debug event");
            }

            Detach();
        }

        public static Thread Run(ref ProcessDebugger pd)
        {
            Thread th = new Thread(new ParameterizedThreadStart(RunnerThread));
            th.Start(pd);
            return th;
        }

        public bool WaitForComeUp(int delay)
        {
            if (isDebugging)
                return true;

            Thread.Sleep(delay);
            return true;
        }

        protected static void RunnerThread(object pd)
        {
            var _pd = (ProcessDebugger)pd;
            _pd.FindAndAttach();
            _pd.StartListener();
            _pd.Detach();
        }
    }
}