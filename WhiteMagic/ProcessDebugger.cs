using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using WhiteMagic.WinAPI;

namespace WhiteMagic
{
    public class DebuggerException : Exception
    {
        public DebuggerException(string message) : base(message) { }
    }

    public class ProcessDebugger : MemoryHandler
    {
        protected volatile bool isDebugging = false;
        protected volatile bool isDetached = false;
        protected volatile bool hasExited = false;
        protected HardwareBreakPoint[] breakPoints = new HardwareBreakPoint[4];
        protected Thread debugThread = null;
        protected int processThreadId = 0;

        public int ThreadId { get { return processThreadId; } }
        public bool IsDebugging { get { return isDebugging; } }
        public bool IsDetached { get { return isDetached; } }
        public bool HasExited { get { return hasExited; } }
        public HardwareBreakPoint[] Breakpoints { get { return breakPoints; } }

        public ProcessDebugger(int processId) : base(processId)
        {
            processThreadId = process.Threads[0].Id;
        }

        public void Attach()
        {
            bool res = false;
            if (!Kernel32.CheckRemoteDebuggerPresent(process.Handle, ref res))
                throw new DebuggerException("Failed to check if remote process is already being debugged");

            if (res)
                throw new DebuggerException("Process is already being debugged by another debugger");

            if (!Kernel32.DebugActiveProcess(process.Id))
                throw new DebuggerException("Failed to start debugging");

            if (!Kernel32.DebugSetProcessKillOnExit(false))
                throw new DebuggerException("Failed to set kill on exit");

            isDebugging = true;
        }

        public IntPtr GetModuleAddress(string moduleName)
        {
            foreach (ProcessModule module in process.Modules)
                if (module.ModuleName.ToLower() == moduleName.ToLower())
                    return module.BaseAddress;

            process.Refresh();
            if (process.HasExited)
                return IntPtr.Zero;

            foreach (ProcessModule module in process.Modules)
                if (module.ModuleName.ToLower() == moduleName.ToLower())
                    return module.BaseAddress;

            return LoadModule(moduleName);
        }

        public IntPtr LoadModule(string name)
        {
            lock ("moduleLoad")
            {
                var hModule = Kernel32.GetModuleHandle("kernel32.dll");
                if (hModule == IntPtr.Zero)
                    hModule = Kernel32.LoadLibraryA("kernel32.dll");
                if (hModule == IntPtr.Zero)
                    throw new DebuggerException("Failed to get kernel32.dll module");

                var funcAddress = Kernel32.GetProcAddress(hModule, "LoadLibraryA");
                var arg = AllocateCString(name);

                var ret = (int)Call(IntPtr.Add(GetModuleAddress("kernel32.dll"), funcAddress.ToInt32() - hModule.ToInt32()), CallingConventionEx.StdCall, arg);
                FreeMemory(arg);
                if (ret <= 0)
                    throw new DebuggerException("Failed to load module '" + name + "'");

                return new IntPtr(ret);
            }
        }

        public void AddBreakPoint(HardwareBreakPoint bp, IntPtr baseAddress)
        {
            var idx = -1;
            for (var i = 0; i < breakPoints.Length; ++i)
                if (breakPoints[i] == null)
                {
                    idx = i;
                    break;
                }

            if (idx == -1)
                throw new DebuggerException("Can't set any more breakpoints");

            bp.SetModuleBase(baseAddress);

            try
            {
                using (var suspender = MakeSuspender())
                {
                    bp.Set(process);
                }
            }
            catch (BreakPointException e)
            {
                throw new DebuggerException(e.Message);
            }

            breakPoints[idx] = bp;
        }

        public void RemoveBreakPoint(IntPtr offset, IntPtr moduleBase)
        {
            var idx = -1;

            for (int i = 0; i < breakPoints.Length; ++i)
                if (breakPoints[i] != null && breakPoints[i].Offset == offset && breakPoints[i].ModuleBase == moduleBase)
                {
                    idx = i;
                    break;
                }

            if (idx == -1)
                return;

            try
            {
                using (var suspender = MakeSuspender())
                {
                    breakPoints[idx].UnSet();
                }
            }
            catch (BreakPointException e)
            {
                throw new DebuggerException(e.Message);
            }

            breakPoints[idx] = null;
        }

        public void RemoveBreakPoints()
        {
            try
            {
                using (var suspender = MakeSuspender())
                {
                    foreach (var bp in breakPoints)
                        if (bp != null)
                            bp.UnSet();
                }

                for (var i = 0; i < breakPoints.Length; ++i)
                    breakPoints[i] = null;
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

            if (!Kernel32.DebugActiveProcessStop(process.Id))
                throw new DebuggerException("Failed to stop process debugging");
        }

        public void StartListener(uint waitInterval = 200)
        {
            var DebugEvent = new DEBUG_EVENT();
            for (; isDebugging; )
            {
                if (!Kernel32.WaitForDebugEvent(ref DebugEvent, waitInterval))
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

                        if (!Kernel32.ContinueDebugEvent(DebugEvent.dwProcessId, DebugEvent.dwThreadId, okEvent ? (uint)DebugContinueStatus.DBG_CONTINUE : (uint)DebugContinueStatus.DBG_EXCEPTION_NOT_HANDLED))
                            throw new DebuggerException("Failed to continue debug event");
                        if (!Kernel32.DebugActiveProcessStop(process.Id))
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

                            var hThread = Kernel32.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, DebugEvent.dwThreadId);
                            if (hThread == IntPtr.Zero)
                                throw new DebuggerException("Failed to open thread");

                            var Context = new CONTEXT();
                            Context.ContextFlags = (uint)CONTEXT_FLAGS.CONTEXT_FULL;
                            if (!Kernel32.GetThreadContext(hThread, ref Context))
                                throw new DebuggerException("Failed to get thread context");

                            HardwareBreakPoint bp = null;
                            foreach (var b in breakPoints)
                                if (b != null && b.Address.ToUInt32() == Context.Eip)
                                {
                                    bp = b;
                                    break;
                                }

                            if (bp == null)
                                break;

                            //Console.WriteLine("Triggered");
                            if (bp.HandleException(ref Context, this) && !Kernel32.SetThreadContext(hThread, ref Context))
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
                    if (!Kernel32.ContinueDebugEvent(DebugEvent.dwProcessId, DebugEvent.dwThreadId, okEvent ? (uint)DebugContinueStatus.DBG_CONTINUE : (uint)DebugContinueStatus.DBG_EXCEPTION_NOT_HANDLED))
                        throw new DebuggerException("Failed to continue debug event");
                    if (!Kernel32.DebugActiveProcessStop(process.Id))
                        throw new DebuggerException("Failed to stop process debugging");
                    return;
                }

                if (!Kernel32.ContinueDebugEvent(DebugEvent.dwProcessId, DebugEvent.dwThreadId, okEvent ? (uint)DebugContinueStatus.DBG_CONTINUE : (uint)DebugContinueStatus.DBG_EXCEPTION_NOT_HANDLED))
                    throw new DebuggerException("Failed to continue debug event");
            }

            Detach();
        }

        public void Run()
        {
            debugThread = new Thread(() =>
                {
                    try
                    {
                        Attach();
                        StartListener();
                    }
                    catch (DebuggerException e)
                    {
                        Console.WriteLine("Debugger exception occured: {0}", e.Message);
                    }
                    try
                    {
                        Detach();
                    }
                    catch (DebuggerException e)
                    {
                        Console.WriteLine("Debugger exception occured: {0}", e.Message);
                    }
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
