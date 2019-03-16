using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures;
using WhiteMagic.Breakpoints;
using WhiteMagic.Processes;

namespace WhiteMagic
{
    public class DebuggerException : MagicException
    {
        public DebuggerException(string Message, params object[] Arguments) : base(Message, Arguments) { }
    }

    public class ProcessDebugger : MemoryHandler
    {
        protected Thread debugThread = null;

        public bool IsDebugging { get; protected set; }
        public bool IsDetached { get; protected set; }
        public bool HasExited => !Process.IsValid;

        public List<HardwareBreakPoint> Breakpoints { get; protected set; } = new List<HardwareBreakPoint>();

        public ProcessDebugger(int ProcessId) : base(ProcessId)
        {
        }

        public ProcessDebugger(RemoteProcess process) : base(process)
        {
        }

        private void Attach()
        {
            bool res = false;
            if (!Kernel32.CheckRemoteDebuggerPresent(Process.Handle, ref res))
                throw new DebuggerException("Failed to check if remote process is already being debugged");

            if (res)
                throw new DebuggerException("Process is already being debugged by another debugger");

            if (!Kernel32.DebugActiveProcess(Process.Id))
                throw new DebuggerException("Failed to start debugging");

            if (!Kernel32.DebugSetProcessKillOnExit(false))
                throw new DebuggerException("Failed to set kill on exit");

            ClearUsedBreakpointSlots();

            IsDebugging = true;
        }

        public void ClearUsedBreakpointSlots()
        {
            RefreshMemory();
            foreach (ProcessThread th in Process.Threads)
            {
                var hThread = Kernel32.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, th.Id);
                if (hThread == IntPtr.Zero)
                    throw new BreakPointException("Can't open thread for access");

                HardwareBreakPoint.UnsetSlotsFromThread(hThread, SlotFlags.All);

                if (!Kernel32.CloseHandle(hThread))
                    throw new BreakPointException("Failed to close thread handle");
            }
        }

        public void AddBreakPoint(HardwareBreakPoint Breakpoint)
        {
            if (Breakpoints.Count >= Kernel32.MaxHardwareBreakpointsCount)
                throw new DebuggerException("Can't set any more breakpoints");

            try
            {
                using (var suspender = MakeSuspender())
                {
                    Breakpoint.Set(this);
                    Breakpoints.Add(Breakpoint);
                }
            }
            catch (BreakPointException e)
            {
                throw new DebuggerException(e.Message);
            }
        }

        public void RemoveBreakPoints()
        {
            try
            {
                using (var suspender = MakeSuspender())
                {
                    foreach (var bp in Breakpoints)
                        bp.UnSet(this);
                }

                Breakpoints.Clear();
            }
            catch (BreakPointException e)
            {
                throw new DebuggerException(e.Message);
            }
        }

        public void StopDebugging() => IsDebugging = false;

        public void Join()
        {
            if (debugThread != null)
                debugThread.Join();
        }

        protected void Detach()
        {
            if (IsDetached)
                return;
            IsDetached = true;

            RefreshMemory();
            if (HasExited)
                return;

            RemoveBreakPoints();

            if (!Kernel32.DebugActiveProcessStop(Process.Id))
                throw new DebuggerException("Failed to stop process debugging");
        }

        private void StartListener(uint WaitInterval = 200)
        {
            var DebugEvent = new DEBUG_EVENT();
            for (; IsDebugging;)
            {
                if (!Kernel32.WaitForDebugEvent(ref DebugEvent, WaitInterval))
                {
                    if (!IsDebugging)
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
                        IsDebugging = false;
                        IsDetached = true;

                        if (!Kernel32.ContinueDebugEvent(DebugEvent.dwProcessId, DebugEvent.dwThreadId, okEvent ? (uint)DebugContinueStatus.DBG_CONTINUE : (uint)DebugContinueStatus.DBG_EXCEPTION_NOT_HANDLED))
                            throw new DebuggerException("Failed to continue debug event");
                        if (!Kernel32.DebugActiveProcessStop(Process.Id))
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
                            Context.ContextFlags = CONTEXT_FLAGS.CONTEXT_FULL;
                            if (!Kernel32.GetThreadContext(hThread, Context))
                                throw new DebuggerException("Failed to get thread context");

                            if (!Breakpoints.Any(e => e != null && e.IsSet && e.Address.ToUInt32() == Context.Eip))
                                break;
                            var bp = Breakpoints.First(e => e != null && e.IsSet && e.Address.ToUInt32() == Context.Eip);

                            var ContextWrapper = new ContextWrapper(this, Context);
                            if (bp.HandleException(ContextWrapper))
                            {
                                if (!Kernel32.SetThreadContext(hThread, ContextWrapper.Context))
                                    throw new DebuggerException("Failed to set thread context");
                            }
                        }
                        break;
                    case DebugEventType.CREATE_THREAD_DEBUG_EVENT:
                        {
                            foreach (var bp in Breakpoints)
                                bp.SetToThread(DebugEvent.CreateThread.hThread, DebugEvent.dwThreadId);
                            break;
                        }
                    case DebugEventType.EXIT_THREAD_DEBUG_EVENT:
                        {
                            foreach (var bp in Breakpoints)
                                bp.UnregisterThread(DebugEvent.dwThreadId);
                            break;
                        }
                    default:
                        break;
                }

                if (!IsDebugging)
                {
                    IsDetached = true;

                    RemoveBreakPoints();
                    if (!Kernel32.ContinueDebugEvent(DebugEvent.dwProcessId, DebugEvent.dwThreadId, okEvent ? (uint)DebugContinueStatus.DBG_CONTINUE : (uint)DebugContinueStatus.DBG_EXCEPTION_NOT_HANDLED))
                        throw new DebuggerException("Failed to continue debug event");
                    if (!Kernel32.DebugActiveProcessStop(Process.Id))
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
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception occured: {0}", e.Message);
                    }
                    try
                    {
                        Detach();
                    }
                    catch (DebuggerException e)
                    {
                        Console.WriteLine("Debugger exception occured: {0}", e.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception occured: {0}", e.Message);
                    }
                });
            debugThread.Start();
        }

        public bool WaitForComeUp(int WaitTime)
        {
            if (IsDebugging)
                return true;

            Thread.Sleep(WaitTime);
            return IsDebugging;
        }

        public bool WaitForComeUp(int WaitTime, int RepeatCount)
        {
            for (int i = 0; i < RepeatCount; ++i)
            {
                if (WaitForComeUp(WaitTime))
                    return true;
            }

            return IsDebugging;
        }

        private bool _CatchSigInt = false;
        public bool CatchSigInt
        {
            get
            {
                return _CatchSigInt;
            }
            set
            {
                _CatchSigInt = value;
                if (_CatchSigInt)
                    SigIntHandler.AddInstance(this);
                else
                    SigIntHandler.RemoveInstance(this);
            }
        }
    }

    public static class SigIntHandler
    {
        private static List<ProcessDebugger> AffectedInstances = new List<ProcessDebugger>();

        static SigIntHandler()
        {
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                if (AffectedInstances.Count > 0)
                {
                    e.Cancel = true;

                    foreach (var Debugger in AffectedInstances)
                        Debugger.StopDebugging();
                }
            };
        }

        public static void AddInstance(ProcessDebugger Debugger) => AffectedInstances.Add(Debugger);

        public static void RemoveInstance(ProcessDebugger Debugger) => AffectedInstances.Remove(Debugger);
    }
}
