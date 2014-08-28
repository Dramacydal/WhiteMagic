using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace WhiteMagic
{
    using BreakPointContainer = List<HardwareBreakPoint>;

    public class DebuggerException : Exception
    {
        public DebuggerException(string message) : base(message) { }
    }

    public class ProcessDebugger : MemoryHandler
    {
        protected volatile bool isDebugging = false;
        protected volatile bool isDetached = false;
        protected volatile bool hasExited = false;
        protected BreakPointContainer breakPoints = new BreakPointContainer();
        protected Thread debugThread = null;
        protected int processThreadId = 0;

        public int ThreadId { get { return processThreadId; } }
        public bool IsDebugging { get { return isDebugging; } }
        public bool IsDetached { get { return isDetached; } }
        public bool HasExited { get { return hasExited; } }
        public BreakPointContainer Breakpoints { get { return breakPoints; } }

        public ProcessDebugger(int processId) : base(processId)
        {
            processThreadId = process.Threads[0].Id;
        }

        public void Attach()
        {
            bool res = false;
            if (!WinApi.CheckRemoteDebuggerPresent(process.Handle, ref res))
                throw new DebuggerException("Failed to check if remote process is already being debugged");

            if (res)
                throw new DebuggerException("Process is already being debugged by another debugger");

            if (!WinApi.DebugActiveProcess(process.Id))
                throw new DebuggerException("Failed to start debugging");

            if (!WinApi.DebugSetProcessKillOnExit(false))
                throw new DebuggerException("Failed to set kill on exit");

            isDebugging = true;
        }

        public uint GetModuleAddress(string moduleName)
        {
            foreach (ProcessModule module in process.Modules)
                if (module.ModuleName.ToLower() == moduleName.ToLower())
                    return (uint)module.BaseAddress;

            process.Refresh();
            if (process.HasExited)
                return 0;

            foreach (ProcessModule module in process.Modules)
                if (module.ModuleName.ToLower() == moduleName.ToLower())
                    return (uint)module.BaseAddress;

            return LoadModule(moduleName);
        }

        public uint LoadModule(string name)
        {
            var hModule = WinApi.GetModuleHandle("kernel32.dll");
            if (hModule == 0)
                hModule = WinApi.LoadLibraryA("kernel32.dll");
            if (hModule == 0)
                throw new DebuggerException("Failed to get kernel32.dll module");

            var funcAddress = WinApi.GetProcAddress(hModule, "LoadLibraryA");
            var arg = AllocateCString(name);

            var ret = Call(GetModuleAddress("kernel32.dll") + funcAddress - hModule, CallingConventionEx.StdCall, arg);
            FreeMemory(arg);
            if (ret == 0)
                throw new DebuggerException("Failed to load module '" + name + "'");

            return ret;
        }

        public void AddBreakPoint(HardwareBreakPoint bp, uint baseAddress)
        {
            int offs = (int)bp.Address;
            if (offs > 0)
                bp.Shift(baseAddress);
            else
                //bp.Shift(WinApi.GetProcAddressOrdinal(moduleBase, (uint)Math.Abs(offs)), true);
                throw new DebuggerException("Function ordinals are not supported");

            try
            {
                bp.Set(ThreadId);
            }
            catch (BreakPointException e)
            {
                throw new DebuggerException(e.Message);
            }

            breakPoints.Add(bp);
        }

        public void RemoveBreakPoint(uint address)
        {
            var bp = breakPoints.Find(b => b.Address == address);
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
            isDebugging = false;
        }

        public void Join()
        {
            if (debugThread != null)
                debugThread.Join();
        }

        protected void Detach()
        {
            if (isDetached)
                return;
            isDetached = true;

            process.Refresh();
            if (process.HasExited)
                return;

            RemoveBreakPoints();

            if (!WinApi.DebugActiveProcessStop(process.Id))
                throw new DebuggerException("Failed to stop process debugging");
        }

        public void StartListener(uint waitInterval = 200)
        {
            var DebugEvent = new DEBUG_EVENT();
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
                        isDebugging = false;
                        isDetached = true;

                        if (!WinApi.ContinueDebugEvent(DebugEvent.dwProcessId, DebugEvent.dwThreadId, okEvent ? (uint)DebugContinueStatus.DBG_CONTINUE : (uint)DebugContinueStatus.DBG_EXCEPTION_NOT_HANDLED))
                            throw new DebuggerException("Failed to continue debug event");
                        if (!WinApi.DebugActiveProcessStop(process.Id))
                            throw new DebuggerException("Failed to stop process debugging");
                        return;
                    case DebugEventType.EXCEPTION_DEBUG_EVENT:
                        //Console.WriteLine("Exception Code: {0:X}", DebugEvent.Exception.ExceptionRecord.ExceptionCode);
                        if (DebugEvent.Exception.ExceptionRecord.ExceptionCode == (uint)ExceptonStatus.STATUS_SINGLE_STEP)
                        {
                            okEvent = true;

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

                            var bp = breakPoints.Find(b => b.Address == Context.Eip &&
                                b.ThreadId == DebugEvent.dwThreadId);
                            if (bp == null)
                                break;

                            //Console.WriteLine("Triggered");
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
                    if (!WinApi.DebugActiveProcessStop(process.Id))
                        throw new DebuggerException("Failed to stop process debugging");
                    return;
                }

                if (!WinApi.ContinueDebugEvent(DebugEvent.dwProcessId, DebugEvent.dwThreadId, okEvent ? (uint)DebugContinueStatus.DBG_CONTINUE : (uint)DebugContinueStatus.DBG_EXCEPTION_NOT_HANDLED))
                    throw new DebuggerException("Failed to continue debug event");
            }

            Detach();
        }

        public void Run()
        {
            debugThread = new Thread(() =>
                {
                    Attach();
                    StartListener();
                    Detach();
                });
            debugThread.Start();
        }

        public bool WaitForComeUp(int delay)
        {
            if (isDebugging)
                return true;

            Thread.Sleep(delay);
            return isDebugging;
        }
    }
}
