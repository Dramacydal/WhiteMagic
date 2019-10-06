using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using DirtyMagic.Exceptions;
using DirtyMagic.Modules;
using DirtyMagic.Pointers;
using DirtyMagic.Processes;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Structures;
using Fasm;

namespace DirtyMagic
{
    public class MemoryHandler : IDisposable
    {
        public RemoteProcess Process { get; protected set; }
        public IntPtr ProcessHandle { get; protected set; }

        private volatile int _threadSuspendCount = 0;
        private volatile List<int> _remoteThreads = new List<int>();

        protected ModuleInfo BaseModule;
        protected Dictionary<string, ModuleInfo> Modules = new Dictionary<string, ModuleInfo>();

        public MemoryHandler(RemoteProcess process)
        {
            SetProcess(process);
        }

        public MemoryHandler(int processId)
        {
            var process = RemoteProcess.GetById(processId);
            if (process == null)
                throw new MemoryException($"Process {processId} not found");
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

        protected void SetProcess(RemoteProcess process)
        {
            if (!Kernel32.Is32BitProcess(process.Handle))
                throw new MemoryException("Can't operate with x64 processes");

            this.Process = process;

            if (ProcessHandle != IntPtr.Zero)
                Kernel32.CloseHandle(ProcessHandle);

            ProcessHandle = Kernel32.OpenProcess(ProcessAccess.AllAccess, false, process.Id);
            RefreshModules();
        }

        protected void RefreshModules()
        {
            if (BaseModule == null)
                BaseModule = new ModuleInfo(Process.MainModule);
            else
                BaseModule.Update(Process.MainModule);

            foreach (var module in Modules)
                module.Value.Invalidate();

            foreach (var processModule in Process.Modules)
                GetModule(processModule.ModuleName, true);
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

        public void SuspendAllThreads(params int[] exceptIds)
        {
            if (++_threadSuspendCount > 1)
                return;

            RefreshMemory();

            foreach (var pT in Process.Threads)
            {
                if (exceptIds.Contains(pT.Id))
                    continue;

                if (_remoteThreads.Contains(pT.Id))
                    continue;

                SuspendThread(pT.Id);
            }
        }

        public void SuspendThread(int threadId)
        {
            var pOpenThread = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, threadId);
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

        public void ResumeAllThreads(bool ignoreSuspensionCount = false)
        {
            if (--_threadSuspendCount > 0)
                return;

            if (!ignoreSuspensionCount && _threadSuspendCount < 0)
                throw new MemoryException($"Wrong thread suspend/resume order. threadSuspendCount is {_threadSuspendCount}");

            foreach (var pT in Process.Threads)
                ResumeThread(pT.Id);
        }

        public void ResumeThread(int threadId)
        {
            var pOpenThread = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, threadId);
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
        public byte[] ReadBytes(IntPtr address, int count)
        {
            var buffer = new byte[count];

            if (!Kernel32.VirtualProtectEx(ProcessHandle, address, count, AllocationProtect.PAGE_EXECUTE_READWRITE, out var oldProtect))
                throw new MemoryException("Failed to set page protection before read in remote process");

            if (!Kernel32.ReadProcessMemory(ProcessHandle, address, buffer, count, out var numBytes) || numBytes != count)
                throw new MemoryException("Failed to read memory in remote process");

            if (!Kernel32.VirtualProtectEx(ProcessHandle, address, count, oldProtect, out _))
                throw new MemoryException("Failed to set page protection after read in remote process");

            return buffer;
        }

        public T[] ReadArray<T>(IntPtr address, int elementsCount)
        {
            var bytes = ReadBytes(address, elementsCount * Marshal.SizeOf(typeof(T)));
            var dest = new T[elementsCount];

            Buffer.BlockCopy(bytes, 0, dest, 0, bytes.Length);

            return dest;
        }

        public T Read<T>(IntPtr address) where T : struct
            => MemoryHelpers.ReinterpretObject<T>(ReadBytes(address, Marshal.SizeOf(typeof(T))));

        protected byte[] ReadNullTerminatedBytes(IntPtr address, int charSize = 1)
        {
            if (charSize == 0)
                throw new MemoryException($"Wrong charsize specified for {nameof(ReadNullTerminatedBytes)}");

            var bytes = new List<byte>();
            for (; ; )
            {
                bool notNull = false;
                for (var i = 0; i < charSize; ++i)
                {
                    var b = ReadByte(address);
                    bytes.Add(b);
                    notNull |= b != 0;

                    address = IntPtr.Add(address, 1);
                }

                if (!notNull)
                {
                    bytes.RemoveRange(bytes.Count - charSize, charSize);
                    break;
                }
            }

            return bytes.ToArray();
        }

        public string ReadASCIIString(IntPtr address, int length = 0)
            => Encoding.ASCII.GetString(length == 0 ? ReadNullTerminatedBytes(address) : ReadBytes(address, length));

        public string ReadUTF8String(IntPtr address, int length = 0)
            => Encoding.UTF8.GetString(length == 0 ? ReadNullTerminatedBytes(address) : ReadBytes(address, length));

        public string ReadUTF16String(IntPtr address, int length = 0)
            => Encoding.Unicode.GetString(length == 0 ? ReadNullTerminatedBytes(address, 2) : ReadBytes(address, length));

        public string ReadUTF32String(IntPtr address, int length = 0)
            => Encoding.UTF32.GetString(length == 0 ? ReadNullTerminatedBytes(address, 4) : ReadBytes(address, length));

        public T Read<T>(ModulePointer pointer) where T : struct => Read<T>(GetAddress(pointer));

        #region Faster Read functions for basic types
        public uint ReadUInt(IntPtr address) => BitConverter.ToUInt32(ReadBytes(address, Marshal.SizeOf(typeof(uint))), 0);

        public int ReadInt(IntPtr address) => BitConverter.ToInt32(ReadBytes(address, Marshal.SizeOf(typeof(int))), 0);

        public ushort ReadUShort(IntPtr address) => BitConverter.ToUInt16(ReadBytes(address, Marshal.SizeOf(typeof(ushort))), 0);

        public short ReadShort(IntPtr address) => BitConverter.ToInt16(ReadBytes(address, Marshal.SizeOf(typeof(short))), 0);

        public ulong ReadULong(IntPtr address) => BitConverter.ToUInt64(ReadBytes(address, Marshal.SizeOf(typeof(ulong))), 0);

        public long ReadLong(IntPtr address) => BitConverter.ToInt64(ReadBytes(address, Marshal.SizeOf(typeof(long))), 0);

        public byte ReadByte(IntPtr address) => ReadBytes(address, Marshal.SizeOf(typeof(byte)))[0];

        public sbyte ReadSByte(IntPtr address) => (sbyte)ReadBytes(address, Marshal.SizeOf(typeof(sbyte)))[0];

        public float ReadSingle(IntPtr address) => BitConverter.ToSingle(ReadBytes(address, Marshal.SizeOf(typeof(float))), 0);

        public double ReadDouble(IntPtr address) => BitConverter.ToDouble(ReadBytes(address, Marshal.SizeOf(typeof(float))), 0);

        public IntPtr ReadPointer(IntPtr address) => new IntPtr(ReadInt(address));

        #endregion
        #endregion

        #region Memory writing
        public void WriteBytes(IntPtr address, byte[] data)
        {
            if (!Kernel32.VirtualProtectEx(ProcessHandle, address, data.Length, AllocationProtect.PAGE_EXECUTE_READWRITE, out var originalProtection))
                throw new MemoryException("Failed to set page protection before write in remote process");

            if (!Kernel32.WriteProcessMemory(ProcessHandle, address, data, data.Length, out var numBytes) || numBytes != data.Length)
                throw new MemoryException("Failed to write memory in remote process");

            if (!Kernel32.VirtualProtectEx(ProcessHandle, address, data.Length, originalProtection, out _))
                throw new MemoryException("Failed to set page protection after write in remote process");
        }

        public void Write<T>(IntPtr address, T value)
        {
            var bytes = MemoryHelpers.ObjectToBytes(value);
            WriteBytes(address, bytes);
        }

        public void WriteCString(IntPtr address, string @string, bool nullTerminated = true)
            => WriteBytes(address, Encoding.ASCII.GetBytes(nullTerminated ? @string + '\0' : @string));

        public void WriteUTF8String(IntPtr address, string @string, bool nullTerminated = true)
            => WriteBytes(address, Encoding.UTF8.GetBytes(nullTerminated ? @string + '\0' : @string));

        public void WriteUTF16String(IntPtr address, string @string, bool nullTerminated = true)
            => WriteBytes(address, Encoding.Unicode.GetBytes(nullTerminated ? @string + '\0' : @string));

        public void WriteUTF32String(IntPtr address, string @string, bool nullTerminated = true)
            => WriteBytes(address, Encoding.UTF32.GetBytes(nullTerminated ? @string + '\0' : @string));

        public void Write<T>(ModulePointer pointer, T value) where T : struct => Write<T>(GetAddress(pointer), value);

        #region Faster Write functions for basic types
        public void WriteUInt(IntPtr address, uint value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteInt(IntPtr address, int value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteUShort(IntPtr address, ushort value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteShort(IntPtr address, short value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteULong(IntPtr address, ulong value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteLong(IntPtr address, long value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteByte(IntPtr address, byte value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteSByte(IntPtr address, sbyte value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteSingle(IntPtr address, float value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteDouble(IntPtr address, double value) => WriteBytes(address, BitConverter.GetBytes(value));
        #endregion
        #endregion

        #region Memory allocators
        public IntPtr AllocateMemory(int size)
        {
            var addr = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, size, AllocationType.Commit | AllocationType.Reserve, AllocationProtect.PAGE_EXECUTE_READWRITE);
            if (addr == 0)
                throw new MemoryException("Failed to allocate memory in remote process");

            return new IntPtr(addr);
        }

        public void FreeMemory(IntPtr address)
        {
            if (!Kernel32.VirtualFreeEx(ProcessHandle, address, 0, FreeType.Release))
                throw new MemoryException("Failed to free memory in remote process");
        }

        public IntPtr AllocateCString(string @string) => AllocateBytes(Encoding.ASCII.GetBytes(@string));

        public IntPtr AllocateUTF8String(string @string) => AllocateBytes(Encoding.UTF8.GetBytes(@string));

        public IntPtr AllocateUTF16String(string @string) => AllocateBytes(Encoding.Unicode.GetBytes(@string));

        public IntPtr AllocateUTF32String(string @string) => AllocateBytes(Encoding.UTF32.GetBytes(@string));

        public IntPtr AllocateBytes(byte[] data)
        {
            var addr = AllocateMemory(data.Length);
            WriteBytes(addr, data);
            return addr;
        }

        public IntPtr Allocate<T>(T @object)
        {
            var size = Marshal.SizeOf(typeof(T));
            var addr = AllocateMemory(size);
            Write<T>(addr, @object);
            return addr;
        }
        #endregion

        public T ExecuteRemoteCode<T>(byte[] byteCode) where T : struct
        {
            var addr = AllocateBytes(byteCode);
            var exitCode = ExecuteRemoteCode<T>(addr);

            FreeMemory(addr);

            return exitCode;
        }

        public T ExecuteRemoteCode<T>(IntPtr address) where T : struct
        {
            lock ("codeExecution")
            {
                var h = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                if (h == IntPtr.Zero)
                    throw new MemoryException("Failed to create remote thread");

                _remoteThreads.Add(threadId);

                if (Kernel32.WaitForSingleObject(h, (uint)WaitResult.INFINITE) != WaitResult.WAIT_OBJECT_0)
                    throw new MemoryException("Failed to wait for remote thread");

                _remoteThreads.Remove(threadId);

                if (!Kernel32.GetExitCodeThread(h, out var exitCode))
                    throw new MemoryException("Failed to obtain exit code");

                return MemoryHelpers.ReinterpretObject<T>(exitCode);
            }
        }

        public T Call<T>(ModulePointer pointer, MagicConvention callingConvention, params object[] arguments) where T : struct
            => Call<T>(GetAddress(pointer), callingConvention, arguments);

        public void Call(ModulePointer pointer, MagicConvention callingConvention, params object[] arguments)
            => Call(GetAddress(pointer), callingConvention, arguments);

        public void Call(IntPtr address, MagicConvention callingConvention, params object[] arguments)
            => Call<int>(address, callingConvention, arguments);

        public T Call<T>(IntPtr address, MagicConvention callingConvention, params object[] arguments) where T : struct
        {
            using (var asm = new ManagedFasm())
            {
                asm.Clear();

                switch (callingConvention)
                {
                    case MagicConvention.Cdecl:
                    {
                        asm.AddLine("push ebp");
                        for (var i = arguments.Length - 1; i >= 0; --i)
                            asm.AddLine("push {0}", arguments[i]);
                        asm.AddLine("mov eax, {0}", address);
                        asm.AddLine("call eax");
                        for (var i = 0; i < arguments.Length; ++i)
                            asm.AddLine("pop ebp");
                        asm.AddLine("pop ebp");

                        asm.AddLine("retn");
                        break;
                    }
                    case MagicConvention.StdCall:
                    {
                        for (var i = arguments.Length - 1; i >= 0; --i)
                            asm.AddLine("push {0}", arguments[i]);
                        asm.AddLine("mov eax, {0}", address);
                        asm.AddLine("call eax");
                        asm.AddLine("retn");
                        break;
                    }
                    case MagicConvention.FastCall:
                    {
                        if (arguments.Length > 0)
                            asm.AddLine("mov ecx, {0}", arguments[0]);
                        if (arguments.Length > 1)
                            asm.AddLine("mov edx, {0}", arguments[1]);
                        for (var i = arguments.Length - 1; i >= 2; --i)
                            asm.AddLine("push {0}", arguments[i]);
                        asm.AddLine("mov eax, {0}", address);
                        asm.AddLine("call eax");
                        asm.AddLine("retn");
                        break;
                    }
                    case MagicConvention.Register:
                    {
                        if (arguments.Length > 0)
                            asm.AddLine("mov eax, {0}", arguments[0]);
                        if (arguments.Length > 1)
                            asm.AddLine("mov edx, {0}", arguments[1]);
                        if (arguments.Length > 2)
                            asm.AddLine("mov ecx, {0}", arguments[2]);
                        for (var i = 3; i < arguments.Length; ++i)
                            asm.AddLine("push {0}", arguments[i]);
                        asm.AddLine("mov ebx, {0}", address);
                        asm.AddLine("call ebx");
                        asm.AddLine("retn");
                        break;
                    }
                    case MagicConvention.ThisCall:
                    {
                        if (arguments.Length > 0)
                            asm.AddLine("mov ecx, {0}", arguments[0]);
                        for (var i = arguments.Length - 1; i >= 1; --i)
                            asm.AddLine("push {0}", arguments[i]);
                        asm.AddLine("mov eax, {0}", address);
                        asm.AddLine("call eax");
                        asm.AddLine("retn");
                        break;
                    }
                    default:
                    {
                        throw new MemoryException($"Unhandled calling convention '{callingConvention}'");
                    }
                }

                return ExecuteRemoteCode<T>(asm.Assemble());
            }
        }

        public IntPtr GetThreadStartAddress(int threadId)
        {
            var hThread = Kernel32.OpenThread(ThreadAccess.QUERY_INFORMATION, false, threadId);
            if (hThread == IntPtr.Zero)
                throw new MemoryException("Failed to open thread");
            var buf = new byte[4];
            try
            {
                var result = Ntdll.NtQueryInformationThread(hThread,
                                 ThreadInfoClass.ThreadQuerySetWin32StartAddress,
                                 buf, buf.Length, IntPtr.Zero);
                if (result != 0)
                    throw new MemoryException($"NtQueryInformationThread failed; NTSTATUS = {result:X8}");
                return new IntPtr(BitConverter.ToInt32(buf, 0));
            }
            finally
            {
                Kernel32.CloseHandle(hThread);
            }
        }

        public ModuleDump GetModuleDump(string moduleName, bool refresh = false)
        {
            var moduleInfo = GetModule(moduleName, refresh);
            if (moduleInfo == null)
                return null;

            if (!moduleInfo.Dump.IsInitialized || refresh)
            {
                using (var suspender = MakeSuspender())
                {
                    moduleInfo.Dump.Read(this);
                }
            }

            return moduleInfo.Dump;
        }

        public ModuleInfo GetModule(string name, bool refresh = false)
        {
            var NameKey = name.ToLower();
            var Module = Modules.ContainsKey(NameKey) ? Modules[NameKey] : null;
            if (!refresh)
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

        public IntPtr GetAddress(ModulePointer pointer) => GetModuleAddress(pointer.ModuleName).Add(pointer.Offset);

        public IntPtr GetModuleAddress(string moduleName)
        {
            if (moduleName == string.Empty)
                moduleName = Process.MainModule.ModuleName;

            var Module = GetModule(moduleName);
            if (Module != null)
                return Module.BaseAddress;

            lock ("process refresh")
            {
                Module = GetModule(moduleName, true);
                if (Module != null)
                    return Module.BaseAddress;

                return LoadModule(moduleName);
            }
        }

        public IntPtr LoadModule(string moduleName)
        {
            lock ("moduleLoad")
            {
                var hModule = Kernel32.LoadLibraryA(moduleName);
                if (hModule == IntPtr.Zero)
                    throw new DebuggerException($"Failed to load {moduleName} module");

                return hModule;
            }
        }
    }
}
