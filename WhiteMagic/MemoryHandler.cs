using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using Fasm;
using WhiteMagic.WinAPI;
using WhiteMagic.Modules;
using WhiteMagic.Pointers;
using WhiteMagic.WinAPI.Structures;
using WhiteMagic.Processes;

namespace WhiteMagic
{
    public class MagicException : Exception
    {
        public ErrorCodes LastError { get; }
        public MagicException(string Message, params object[] Arguments) : base(string.Format(Message, Arguments)) { LastError = (ErrorCodes)Marshal.GetLastWin32Error(); }
    }

    public class MemoryException : MagicException
    {
        public MemoryException(string Message, params object[] Arguments) : base(Message, Arguments) { }
    }

    public enum MagicConvention
    {
        Cdecl = 1,
        StdCall = 2,
        ThisCall = 3,
        FastCall = 4,
        Register = 5,   // borland fastcall
    }

    public class MemoryHandler : IDisposable
    {
        public RemoteProcess Process { get; protected set; }
        public IntPtr ProcessHandle { get; protected set; }

        protected volatile int threadSuspendCount = 0;
        protected volatile List<int> remoteThreads = new List<int>();

        protected ModuleInfo BaseModule;
        protected Dictionary<string, ModuleInfo> Modules = new Dictionary<string, ModuleInfo>();

        public MemoryHandler(RemoteProcess Process)
        {
            SetProcess(Process);
        }

        public MemoryHandler(int ProcessId)
        {
            var process = RemoteProcess.GetById(ProcessId);
            if (process == null)
                throw new MemoryException("Process {0} not found", ProcessId);
            SetProcess(process);
        }

        public void Dispose()
        {
            if (ProcessHandle != IntPtr.Zero)
            {
                Kernel32.CloseHandle(ProcessHandle);
                ProcessHandle = IntPtr.Zero;
            }
        }

        ~MemoryHandler()
        {
            Dispose();
        }

        protected void SetProcess(RemoteProcess Process)
        {
            if (!Kernel32.Is32BitProcess(Process.Handle))
                throw new MemoryException("Can't operate with x64 processes");

            this.Process = Process;

            if (ProcessHandle != IntPtr.Zero)
                Kernel32.CloseHandle(ProcessHandle);

            ProcessHandle = Kernel32.OpenProcess(ProcessAccess.AllAccess, false, Process.Id);
            RefreshModules();
        }

        protected void RefreshModules()
        {
            if (BaseModule == null)
                BaseModule = new ModuleInfo(Process.MainModule);
            else
                BaseModule.Update(Process.MainModule);

            foreach (var Module in Modules)
                Module.Value.Invalidate();

            foreach (var ProcessModule in Process.Modules)
                GetModule(ProcessModule.ModuleName, true);
        }

        public bool IsValid
        {
            get
            {
                {
                    if (Process == null)
                        return false;

                    RefreshMemory();

                    return Process.IsValid;
                }
            }
        }

        public void RefreshMemory()
        {
            if (Process == null)
                return;

            lock ("process refresh")
            {
                Process.Refresh();
                RefreshModules();
            }
        }

        public void SuspendAllThreads(params int[] ExceptIds)
        {
            if (++threadSuspendCount > 1)
                return;

            RefreshMemory();

            foreach (var pT in Process.Threads)
            {
                if (ExceptIds.Contains(pT.Id))
                    continue;

                if (remoteThreads.Contains(pT.Id))
                    continue;

                SuspendThread(pT.Id);
            }
        }

        public void SuspendThread(int ThreadId)
        {
            var pOpenThread = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, ThreadId);
            if (pOpenThread == IntPtr.Zero)
            {
                // thread does not exist
                if (Marshal.GetLastWin32Error() == (int)ErrorCodes.ERROR_INVALID_PARAMETER)
                    return;
                throw new MemoryException("Failed to open thread to suspend.");
            }

            var res = Kernel32.SuspendThread(pOpenThread);
            if (res == -1)
                throw new MemoryException("Failed to suspend thread.");

            Kernel32.CloseHandle(pOpenThread);
        }

        public void ResumeAllThreads(bool IgnoreSuspensionCount = false)
        {
            if (--threadSuspendCount > 0)
                return;

            if (!IgnoreSuspensionCount && threadSuspendCount < 0)
                throw new MemoryException("Wrong thread suspend/resume order. threadSuspendCount is {0}", threadSuspendCount.ToString());

            foreach (var pT in Process.Threads)
                ResumeThread(pT.Id);
        }

        public void ResumeThread(int ThreadId)
        {
            var pOpenThread = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, ThreadId);
            if (pOpenThread == IntPtr.Zero)
            {
                // thread does not exist
                if (Marshal.GetLastWin32Error() == (int)ErrorCodes.ERROR_INVALID_PARAMETER)
                    return;
                throw new MemoryException("Failed to open thread to resume.");
            }

            var suspendCount = 0;
            do
            {
                suspendCount = Kernel32.ResumeThread(pOpenThread);
                if (suspendCount == -1)
                    throw new MemoryException("Failed to resume thread.");
            }
            while (suspendCount > 0);

            Kernel32.CloseHandle(pOpenThread);
        }

        #region Memory reading
        public byte[] ReadBytes(IntPtr Address, int Count)
        {
            var buffer = new byte[Count];

            AllocationProtect oldProtect, oldProtect2;
            if (!Kernel32.VirtualProtectEx(ProcessHandle, Address, Count, AllocationProtect.PAGE_EXECUTE_READWRITE, out oldProtect))
                throw new MemoryException("Failed to set page protection before read in remote process");

            int numBytes;
            if (!Kernel32.ReadProcessMemory(ProcessHandle, Address, buffer, Count, out numBytes) || numBytes != Count)
                throw new MemoryException("Failed to read memory in remote process");

            if (!Kernel32.VirtualProtectEx(ProcessHandle, Address, Count, oldProtect, out oldProtect2))
                throw new MemoryException("Failed to set page protection after read in remote process");

            return buffer;
        }

        public T[] ReadArray<T>(IntPtr Address, int ElementsCount)
        {
            var bytes = ReadBytes(Address, ElementsCount * Marshal.SizeOf(typeof(T)));
            var dest = new T[ElementsCount];

            Buffer.BlockCopy(bytes, 0, dest, 0, bytes.Length);

            return dest;
        }

        public T Read<T>(IntPtr Address) where T : struct
            => MagicHelpers.ReinterpretObject<T>(ReadBytes(Address, Marshal.SizeOf(typeof(T))));

        protected byte[] ReadNullTerminatedBytes(IntPtr Address, int CharSize = 1)
        {
            if (CharSize == 0)
                throw new MemoryException($"Wrong charsize specified for {nameof(ReadNullTerminatedBytes)}");

            var bytes = new List<byte>();
            for (; ; )
            {
                bool notNull = false;
                for (var i = 0; i < CharSize; ++i)
                {
                    var b = ReadByte(Address);
                    bytes.Add(b);
                    notNull |= b != 0;

                    Address = IntPtr.Add(Address, 1);
                }
                if (!notNull)
                {
                    bytes.RemoveRange(bytes.Count - CharSize, CharSize);
                    break;
                }
            }

            return bytes.ToArray();
        }

        public string ReadASCIIString(IntPtr Address, int Length = 0)
            => Encoding.ASCII.GetString(Length == 0 ? ReadNullTerminatedBytes(Address) : ReadBytes(Address, Length));

        public string ReadUTF8String(IntPtr Address, int Length = 0)
            => Encoding.UTF8.GetString(Length == 0 ? ReadNullTerminatedBytes(Address) : ReadBytes(Address, Length));

        public string ReadUTF16String(IntPtr Address, int Length = 0)
            => Encoding.Unicode.GetString(Length == 0 ? ReadNullTerminatedBytes(Address, 2) : ReadBytes(Address, Length));

        public string ReadUTF32String(IntPtr Address, int Length = 0)
            => Encoding.UTF32.GetString(Length == 0 ? ReadNullTerminatedBytes(Address, 4) : ReadBytes(Address, Length));

        public T Read<T>(ModulePointer Pointer) where T : struct => Read<T>(GetAddress(Pointer));

        #region Faster Read functions for basic types
        public uint ReadUInt(IntPtr Address) => BitConverter.ToUInt32(ReadBytes(Address, Marshal.SizeOf(typeof(uint))), 0);

        public int ReadInt(IntPtr Address) => BitConverter.ToInt32(ReadBytes(Address, Marshal.SizeOf(typeof(int))), 0);

        public ushort ReadUShort(IntPtr Address) => BitConverter.ToUInt16(ReadBytes(Address, Marshal.SizeOf(typeof(ushort))), 0);

        public short ReadShort(IntPtr Address) => BitConverter.ToInt16(ReadBytes(Address, Marshal.SizeOf(typeof(short))), 0);

        public ulong ReadULong(IntPtr Address) => BitConverter.ToUInt64(ReadBytes(Address, Marshal.SizeOf(typeof(ulong))), 0);

        public long ReadLong(IntPtr Address) => BitConverter.ToInt64(ReadBytes(Address, Marshal.SizeOf(typeof(long))), 0);

        public byte ReadByte(IntPtr Address) => ReadBytes(Address, Marshal.SizeOf(typeof(byte)))[0];

        public sbyte ReadSByte(IntPtr Address) => (sbyte)ReadBytes(Address, Marshal.SizeOf(typeof(sbyte)))[0];

        public float ReadSingle(IntPtr Address) => BitConverter.ToSingle(ReadBytes(Address, Marshal.SizeOf(typeof(float))), 0);

        public double ReadDouble(IntPtr Address) => BitConverter.ToDouble(ReadBytes(Address, Marshal.SizeOf(typeof(float))), 0);

        public IntPtr ReadPointer(IntPtr Address) => new IntPtr(ReadInt(Address));

        #endregion
        #endregion

        #region Memory writing
        public void WriteBytes(IntPtr Address, byte[] Data)
        {
            AllocationProtect originalProtection, tmpProtection;
            if (!Kernel32.VirtualProtectEx(ProcessHandle, Address, Data.Length, AllocationProtect.PAGE_EXECUTE_READWRITE, out originalProtection))
                throw new MemoryException("Failed to set page protection before write in remote process");

            int numBytes;
            if (!Kernel32.WriteProcessMemory(ProcessHandle, Address, Data, Data.Length, out numBytes) || numBytes != Data.Length)
                throw new MemoryException("Failed to write memory in remote process");

            if (!Kernel32.VirtualProtectEx(ProcessHandle, Address, Data.Length, originalProtection, out tmpProtection))
                throw new MemoryException("Failed to set page protection after write in remote process");
        }

        public void Write<T>(IntPtr Address, T Value)
        {
            var bytes = MagicHelpers.ObjectToBytes(Value);
            WriteBytes(Address, bytes);
        }

        public void WriteCString(IntPtr Address, string String, bool DoNullTermination = true)
            => WriteBytes(Address, Encoding.ASCII.GetBytes(DoNullTermination ? String + '\0' : String));

        public void WriteUTF8String(IntPtr Address, string String, bool DoNullTermination = true)
            => WriteBytes(Address, Encoding.UTF8.GetBytes(DoNullTermination ? String + '\0' : String));

        public void WriteUTF16String(IntPtr Address, string String, bool DoNullTermination = true)
            => WriteBytes(Address, Encoding.Unicode.GetBytes(DoNullTermination ? String + '\0' : String));

        public void WriteUTF32String(IntPtr Address, string String, bool DoNullTermination = true)
            => WriteBytes(Address, Encoding.UTF32.GetBytes(DoNullTermination ? String + '\0' : String));

        public void Write<T>(ModulePointer Pointer, T Value) where T : struct => Write<T>(GetAddress(Pointer), Value);

        #region Faster Write functions for basic types
        public void WriteUInt(IntPtr Address, uint Value) => WriteBytes(Address, BitConverter.GetBytes(Value));

        public void WriteInt(IntPtr Address, int Value) => WriteBytes(Address, BitConverter.GetBytes(Value));

        public void WriteUShort(IntPtr Address, ushort Value) => WriteBytes(Address, BitConverter.GetBytes(Value));

        public void WriteShort(IntPtr Address, short Value) => WriteBytes(Address, BitConverter.GetBytes(Value));

        public void WriteULong(IntPtr Address, ulong Value) => WriteBytes(Address, BitConverter.GetBytes(Value));

        public void WriteLong(IntPtr Address, long Value) => WriteBytes(Address, BitConverter.GetBytes(Value));

        public void WriteByte(IntPtr Address, byte Value) => WriteBytes(Address, BitConverter.GetBytes(Value));

        public void WriteSByte(IntPtr Address, sbyte Value) => WriteBytes(Address, BitConverter.GetBytes(Value));

        public void WriteSingle(IntPtr Address, float Value) => WriteBytes(Address, BitConverter.GetBytes(Value));

        public void WriteDouble(IntPtr Address, double Value) => WriteBytes(Address, BitConverter.GetBytes(Value));
        #endregion
        #endregion

        #region Memory allocators
        public IntPtr AllocateMemory(int Size)
        {
            var addr = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, Size, AllocationType.Commit | AllocationType.Reserve, AllocationProtect.PAGE_EXECUTE_READWRITE);
            if (addr == 0)
                throw new MemoryException("Failed to allocate memory in remote process");

            return new IntPtr(addr);
        }

        public void FreeMemory(IntPtr Address)
        {
            if (!Kernel32.VirtualFreeEx(ProcessHandle, Address, 0, FreeType.Release))
                throw new MemoryException("Failed to free memory in remote process");
        }

        public IntPtr AllocateCString(string String) => AllocateBytes(Encoding.ASCII.GetBytes(String));

        public IntPtr AllocateUTF8String(string String) => AllocateBytes(Encoding.UTF8.GetBytes(String));

        public IntPtr AllocateUTF16String(string String) => AllocateBytes(Encoding.Unicode.GetBytes(String));

        public IntPtr AllocateUTF32String(string String) => AllocateBytes(Encoding.UTF32.GetBytes(String));

        public IntPtr AllocateBytes(byte[] Data)
        {
            var addr = AllocateMemory(Data.Length);
            WriteBytes(addr, Data);
            return addr;
        }

        public IntPtr Allocate<T>(T Object)
        {
            var size = Marshal.SizeOf(typeof(T));
            var addr = AllocateMemory(size);
            Write<T>(addr, Object);
            return addr;
        }
        #endregion

        public T ExecuteRemoteCode<T>(byte[] ByteCode) where T : struct
        {
            var addr = AllocateBytes(ByteCode);
            var exitCode = ExecuteRemoteCode<T>(addr);

            FreeMemory(addr);

            return exitCode;
        }

        public T ExecuteRemoteCode<T>(IntPtr Address) where T : struct
        {
            lock ("codeExecution")
            {
                int threadId;
                var h = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, Address, IntPtr.Zero, 0, out threadId);
                if (h == IntPtr.Zero)
                    throw new MemoryException("Failed to create remote thread");

                remoteThreads.Add(threadId);

                if (Kernel32.WaitForSingleObject(h, (uint)WaitResult.INFINITE) != WaitResult.WAIT_OBJECT_0)
                    throw new MemoryException("Failed to wait for remote thread");

                remoteThreads.Remove(threadId);

                uint exitCode;
                if (!Kernel32.GetExitCodeThread(h, out exitCode))
                    throw new MemoryException("Failed to obtain exit code");

                return MagicHelpers.ReinterpretObject<T>(exitCode);
            }
        }

        public T Call<T>(ModulePointer Pointer, MagicConvention CallingConvention, params object[] Arguments) where T : struct
            => Call<T>(GetAddress(Pointer), CallingConvention, Arguments);

        public void Call(ModulePointer Pointer, MagicConvention CallingConvention, params object[] Arguments)
            => Call(GetAddress(Pointer), CallingConvention, Arguments);

        public void Call(IntPtr Address, MagicConvention CallingConvention, params object[] Arguments)
            => Call<int>(Address, CallingConvention, Arguments);

        public T Call<T>(IntPtr Address, MagicConvention CallingConvention, params object[] Arguments) where T : struct
        {
            using (var asm = new ManagedFasm())
            {
                asm.Clear();

                switch (CallingConvention)
                {
                    case MagicConvention.Cdecl:
                    {
                        asm.AddLine("push ebp");
                        for (var i = Arguments.Length - 1; i >= 0; --i)
                            asm.AddLine("push {0}", Arguments[i]);
                        asm.AddLine("mov eax, {0}", Address);
                        asm.AddLine("call eax");
                        for (var i = 0; i < Arguments.Length; ++i)
                            asm.AddLine("pop ebp");
                        asm.AddLine("pop ebp");

                        asm.AddLine("retn");
                        break;
                    }
                    case MagicConvention.StdCall:
                    {
                        for (var i = Arguments.Length - 1; i >= 0; --i)
                            asm.AddLine("push {0}", Arguments[i]);
                        asm.AddLine("mov eax, {0}", Address);
                        asm.AddLine("call eax");
                        asm.AddLine("retn");
                        break;
                    }
                    case MagicConvention.FastCall:
                    {
                        if (Arguments.Length > 0)
                            asm.AddLine("mov ecx, {0}", Arguments[0]);
                        if (Arguments.Length > 1)
                            asm.AddLine("mov edx, {0}", Arguments[1]);
                        for (var i = Arguments.Length - 1; i >= 2; --i)
                            asm.AddLine("push {0}", Arguments[i]);
                        asm.AddLine("mov eax, {0}", Address);
                        asm.AddLine("call eax");
                        asm.AddLine("retn");
                        break;
                    }
                    case MagicConvention.Register:
                    {
                        if (Arguments.Length > 0)
                            asm.AddLine("mov eax, {0}", Arguments[0]);
                        if (Arguments.Length > 1)
                            asm.AddLine("mov edx, {0}", Arguments[1]);
                        if (Arguments.Length > 2)
                            asm.AddLine("mov ecx, {0}", Arguments[2]);
                        for (var i = 3; i < Arguments.Length; ++i)
                            asm.AddLine("push {0}", Arguments[i]);
                        asm.AddLine("mov ebx, {0}", Address);
                        asm.AddLine("call ebx");
                        asm.AddLine("retn");
                        break;
                    }
                    case MagicConvention.ThisCall:
                    {
                        if (Arguments.Length > 0)
                            asm.AddLine("mov ecx, {0}", Arguments[0]);
                        for (var i = Arguments.Length - 1; i >= 1; --i)
                            asm.AddLine("push {0}", Arguments[i]);
                        asm.AddLine("mov eax, {0}", Address);
                        asm.AddLine("call eax");
                        asm.AddLine("retn");
                        break;
                    }
                    default:
                    {
                        throw new MemoryException("Unhandled calling convention '{0}'", CallingConvention.ToString());
                    }
                }

                return ExecuteRemoteCode<T>(asm.Assemble());
            }
        }

        public IntPtr GetThreadStartAddress(int ThreadId)
        {
            var hThread = Kernel32.OpenThread(ThreadAccess.QUERY_INFORMATION, false, ThreadId);
            if (hThread == IntPtr.Zero)
                throw new MemoryException("Failed to open thread");
            var buf = new byte[4];
            try
            {
                var result = Ntdll.NtQueryInformationThread(hThread,
                                 ThreadInfoClass.ThreadQuerySetWin32StartAddress,
                                 buf, buf.Length, IntPtr.Zero);
                if (result != 0)
                    throw new MemoryException("NtQueryInformationThread failed; NTSTATUS = {0:X8}", result);
                return new IntPtr(BitConverter.ToInt32(buf, 0));
            }
            finally
            {
                Kernel32.CloseHandle(hThread);
            }
        }

        public ModuleDump GetModuleDump(string ModuleName, bool Refresh = false)
        {
            var moduleInfo = GetModule(ModuleName, Refresh);
            if (moduleInfo == null)
                return null;

            if (!moduleInfo.Dump.Initialized || Refresh)
            {
                using (var suspender = MakeSuspender())
                {
                    moduleInfo.Dump.Read(this);
                }
            }

            return moduleInfo.Dump;
        }

        public ModuleInfo GetModule(string Name, bool Refresh = false)
        {
            var NameKey = Name.ToLower();
            var Module = Modules.ContainsKey(NameKey) ? Modules[NameKey] : null;
            if (!Refresh)
                return Module;

            try
            {
                var ModuleSource = Process.Modules.Cast<ProcessModule>().First(_ => _.ModuleName.Equals(NameKey, StringComparison.InvariantCultureIgnoreCase));

                if (Module != null)
                    Module.Update(ModuleSource);
                else
                {
                    Module = new ModuleInfo(ModuleSource);
                    Modules[NameKey] = Module;
                }

                return Module;
            }
            catch
            {
                return null;
            }
        }

        public ProcessSuspender MakeSuspender() => new ProcessSuspender(this);

        public IntPtr GetAddress(ModulePointer Pointer) => GetModuleAddress(Pointer.ModuleName).Add(Pointer.Offset);

        public IntPtr GetModuleAddress(string ModuleName)
        {
            if (ModuleName == string.Empty)
                ModuleName = Process.MainModule.ModuleName;

            var Module = GetModule(ModuleName);
            if (Module != null)
                return Module.BaseAddress;

            lock ("process refresh")
            {
                Module = GetModule(ModuleName, true);
                if (Module != null)
                    return Module.BaseAddress;

                return LoadModule(ModuleName);
            }
        }

        public IntPtr LoadModule(string ModuleName)
        {
            lock ("moduleLoad")
            {
                var hModule = Kernel32.LoadLibraryA(ModuleName);
                if (hModule == IntPtr.Zero)
                    throw new DebuggerException($"Failed to load {ModuleName} module");

                return hModule;
            }
        }
    }
}
