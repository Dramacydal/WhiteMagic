using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using Fasm;
using System.Runtime.CompilerServices;
using WhiteMagic.WinAPI;

namespace WhiteMagic
{
    public class MemoryException : Exception
    {
        public MemoryException(string message) : base(message) { }
    }

    public enum CallingConventionEx
    {
        Cdecl = 1,
        StdCall = 2,
        ThisCall = 3,
        FastCall = 4,
        Register = 5,   // borland fastcall
    }

    public class MemoryHandler : IDisposable
    {
        public Process Process { get { return process; } }
        public IntPtr ProcessHandle { get { return processHandle; } }

        protected IntPtr processHandle;
        protected Process process;

        protected volatile int threadSuspendCount = 0;
        protected volatile List<int> remoteThreads = new List<int>();

        protected Dictionary<string, ModuleDump> moduleDump = new Dictionary<string, ModuleDump>();

        public MemoryHandler(Process process)
        {
            SetProcess(process);
        }

        public MemoryHandler(int processId)
        {
            var process = Process.GetProcessById(processId);
            if (process == null)
                throw new MemoryException("Process " + processId + " not found");
            SetProcess(process);
        }

        public void Dispose()
        {
            if (processHandle != IntPtr.Zero)
            {
                Kernel32.CloseHandle(processHandle);
                processHandle = IntPtr.Zero;
            }
        }

        ~MemoryHandler()
        {
            Dispose();
        }

        public void SetProcess(Process process)
        {
            this.process = process;
            if (processHandle != IntPtr.Zero)
                Kernel32.CloseHandle(processHandle);

            processHandle = Kernel32.OpenProcess(ProcessAccess.AllAccess, false, process.Id);
        }

        public bool IsValid()
        {
            if (process == null)
                return false;

            process.Refresh();

            return !process.HasExited;
        }

        public void SuspendAllThreads(params int[] except)
        {
            if (++threadSuspendCount > 1)
                return;

            process.Refresh();

            foreach (ProcessThread pT in process.Threads)
            {
                if (except.Contains(pT.Id))
                    continue;

                if (remoteThreads.Contains(pT.Id))
                    continue;

                SuspendThread(pT.Id);
            }
        }

        public void SuspendThread(int id)
        {
            var pOpenThread = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, id);
            if (pOpenThread == IntPtr.Zero)
                return;

            Kernel32.SuspendThread(pOpenThread);

            Kernel32.CloseHandle(pOpenThread);
        }

        public void ResumeAllThreads(bool ignoreSuspendCount = false)
        {
            if (--threadSuspendCount > 0)
                return;

            if (!ignoreSuspendCount && threadSuspendCount < 0)
                throw new MemoryException("Wrong thread suspend/resume order. threadSuspendCount is " + threadSuspendCount.ToString());

            foreach (ProcessThread pT in process.Threads)
                ResumeThread(pT.Id);
        }

        public void ResumeThread(int id)
        {
            var pOpenThread = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, id);
            if (pOpenThread == IntPtr.Zero)
                return;

            var suspendCount = 0;
            do
            {
                suspendCount = Kernel32.ResumeThread(pOpenThread);
            }
            while (suspendCount > 0);

            Kernel32.CloseHandle(pOpenThread);
        }

        #region Memory reading
        public byte[] ReadBytes(uint addr, int count)
        {
            var buf = new byte[count];

            AllocationProtect oldProtect, oldProtect2;
            if (!Kernel32.VirtualProtectEx(processHandle, (IntPtr)addr, count, AllocationProtect.PAGE_EXECUTE_READWRITE, out oldProtect))
                throw new MemoryException("Failed to set page protection before read in remote process");

            int numBytes;
            if (!Kernel32.ReadProcessMemory(processHandle, (IntPtr)addr, buf, count, out numBytes) || numBytes != count)
                throw new MemoryException("Failed to read memory in remote process");

            if (!Kernel32.VirtualProtectEx(processHandle, (IntPtr)addr, count, oldProtect, out oldProtect2))
                throw new MemoryException("Failed to set page protection after read in remote process");

            return buf;
        }

        public T[] ReadArray<T>(uint addr, int count)
        {
            var bytes = ReadBytes(addr, count * Marshal.SizeOf(typeof(T)));
            var dest = new T[count];

            Buffer.BlockCopy(bytes, 0, dest, 0, bytes.Length);

            return dest;
        }

        public T Read<T>(uint addr)
        {
            T t;
            var buf = ReadBytes(addr, Marshal.SizeOf(typeof(T)));

            var h = GCHandle.Alloc(buf, GCHandleType.Pinned);
            t = (T)Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof(T));
            h.Free();

            return t;
        }

        protected byte[] ReadNullTerminatedBytes(uint addr, int step = 1)
        {
            if (step == 0)
                throw new MemoryException("Wrong step specified for ReadNullTerminatedBytes");

            var bytes = new List<byte>();
            for (; ; )
            {
                bool notNull = false;
                for (var i = 0; i < step; ++i)
                {
                    var b = ReadByte(addr++);
                    bytes.Add(b);
                    notNull |= b != 0;
                }
                if (!notNull)
                {
                    bytes.RemoveRange(bytes.Count - step, step);
                    break;
                }
            }

            return bytes.ToArray();
        }

        public string ReadASCIIString(uint addr, int len = 0)
        {
            return Encoding.ASCII.GetString(len == 0 ? ReadNullTerminatedBytes(addr) : ReadBytes(addr, len));
        }

        public string ReadUTF8String(uint addr, int len = 0)
        {
            return Encoding.UTF8.GetString(len == 0 ? ReadNullTerminatedBytes(addr) : ReadBytes(addr, len));
        }

        public string ReadUTF16String(uint addr, int len = 0)
        {
            return Encoding.Unicode.GetString(len == 0 ? ReadNullTerminatedBytes(addr, 2) : ReadBytes(addr, len));
        }

        public string ReadUTF32String(uint addr, int len = 0)
        {
            return Encoding.UTF32.GetString(len == 0 ? ReadNullTerminatedBytes(addr, 4) : ReadBytes(addr, len));
        }

        #region Faster Read functions for basic types
        public uint ReadUInt(uint addr)
        {
            var buf = ReadBytes(addr, Marshal.SizeOf(typeof(uint)));

            return BitConverter.ToUInt32(buf, 0);
        }

        public int ReadInt(uint addr)
        {
            var buf = ReadBytes(addr, Marshal.SizeOf(typeof(int)));

            return BitConverter.ToInt32(buf, 0);
        }

        public ushort ReadUShort(uint addr)
        {
            var buf = ReadBytes(addr, Marshal.SizeOf(typeof(ushort)));

            return BitConverter.ToUInt16(buf, 0);
        }

        public short ReadShort(uint addr)
        {
            var buf = ReadBytes(addr, Marshal.SizeOf(typeof(short)));

            return BitConverter.ToInt16(buf, 0);
        }

        public ulong ReadULong(uint addr)
        {
            var buf = ReadBytes(addr, Marshal.SizeOf(typeof(ulong)));

            return BitConverter.ToUInt64(buf, 0);
        }

        public long ReadLong(uint addr)
        {
            var buf = ReadBytes(addr, Marshal.SizeOf(typeof(long)));

            return BitConverter.ToInt64(buf, 0);
        }

        public byte ReadByte(uint addr)
        {
            var buf = ReadBytes(addr, Marshal.SizeOf(typeof(byte)));

            return buf[0];
        }

        public sbyte ReadSByte(uint addr)
        {
            var buf = ReadBytes(addr, Marshal.SizeOf(typeof(sbyte)));

            return (sbyte)buf[0];
        }
        #endregion
        #endregion

        #region Memory writing
        public void WriteBytes(uint addr, byte[] bytes)
        {
            AllocationProtect oldProtect, oldProtect2;
            if (!Kernel32.VirtualProtectEx(processHandle, (IntPtr)addr, bytes.Length, AllocationProtect.PAGE_EXECUTE_READWRITE, out oldProtect))
                throw new MemoryException("Failed to set page protection before write in remote process");

            int numBytes;
            if (!Kernel32.WriteProcessMemory(processHandle, (IntPtr)addr, bytes, bytes.Length, out numBytes) || numBytes != bytes.Length)
                throw new MemoryException("Failed to write memory in remote process");

            if (!Kernel32.VirtualProtectEx(processHandle, (IntPtr)addr, bytes.Length, oldProtect, out oldProtect2))
                throw new MemoryException("Failed to set page protection after write in remote process");
        }

        public void Write<T>(uint addr, T value)
        {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            WriteBytes(addr, bytes);
        }

        public void WriteCString(uint addr, string str)
        {
            WriteBytes(addr, Encoding.ASCII.GetBytes(str + "\0"));
        }

        public void WriteUTF8String(uint addr, string str)
        {
            WriteBytes(addr, Encoding.UTF8.GetBytes(str + "\0"));
        }

        public void WriteUTF16String(uint addr, string str)
        {
            WriteBytes(addr, Encoding.Unicode.GetBytes(str + "\0"));
        }

        public void WriteUTF32String(uint addr, string str)
        {
            WriteBytes(addr, Encoding.UTF32.GetBytes(str + "\0"));
        }

        #region Faster Write functions for basic types
        public void WriteUInt(uint addr, uint value)
        {
            WriteBytes(addr, BitConverter.GetBytes(value));
        }

        public void WriteInt(uint addr, int value)
        {
            WriteBytes(addr, BitConverter.GetBytes(value));
        }

        public void WriteUShort(uint addr, ushort value)
        {
            WriteBytes(addr, BitConverter.GetBytes(value));
        }

        public void WriteShort(uint addr, short value)
        {
            WriteBytes(addr, BitConverter.GetBytes(value));
        }

        public void WriteULong(uint addr, ulong value)
        {
            WriteBytes(addr, BitConverter.GetBytes(value));
        }

        public void WriteLong(uint addr, long value)
        {
            WriteBytes(addr, BitConverter.GetBytes(value));
        }

        public void WriteByte(uint addr, byte value)
        {
            WriteBytes(addr, BitConverter.GetBytes(value));
        }

        public void WriteSByte(uint addr, sbyte value)
        {
            WriteBytes(addr, BitConverter.GetBytes(value));
        }
        #endregion
        #endregion

        #region Memory allocators
        public uint AllocateMemory(int size)
        {
            var addr = Kernel32.VirtualAllocEx(processHandle, IntPtr.Zero, size, AllocationType.Commit | AllocationType.Reserve, AllocationProtect.PAGE_EXECUTE_READWRITE);
            if (addr == 0)
                throw new MemoryException("Failed to allocate memory in remote process");

            return addr;
        }

        public void FreeMemory(uint addr)
        {
            if (!Kernel32.VirtualFreeEx(processHandle, (IntPtr)addr, 0, FreeType.Release))
                throw new MemoryException("Failed to free memory in remote process");
        }

        public uint AllocateCString(string str)
        {
            return AllocateBytes(Encoding.ASCII.GetBytes(str));
        }

        public uint AllocateUTF8String(string str)
        {
            return AllocateBytes(Encoding.UTF8.GetBytes(str));
        }

        public uint AllocateUTF16String(string str)
        {
            return AllocateBytes(Encoding.Unicode.GetBytes(str));
        }

        public uint AllocateUTF32String(string str)
        {
            return AllocateBytes(Encoding.UTF32.GetBytes(str));
        }

        public uint AllocateBytes(byte[] bytes)
        {
            var addr = AllocateMemory(bytes.Length);
            WriteBytes(addr, bytes);
            return addr;
        }

        public uint Allocate<T>(T obj)
        {
            var size = Marshal.SizeOf(typeof(T));
            var addr = AllocateMemory(size);
            Write<T>(addr, obj);
            return addr;
        }
        #endregion

        public uint ExecuteRemoteCode(byte[] bytes)
        {
            var addr = AllocateBytes(bytes);
            var exitCode = ExecuteRemoteCode(addr);

            FreeMemory(addr);

            return exitCode;
        }

        public uint ExecuteRemoteCode(uint addr)
        {
            lock ("codeExecution")
            {
                int threadId;
                var h = Kernel32.CreateRemoteThread(processHandle, IntPtr.Zero, 0, addr, IntPtr.Zero, 0, out threadId);
                if (h == IntPtr.Zero)
                    throw new MemoryException("Failed to create remote thread");

                remoteThreads.Add(threadId);

                if (Kernel32.WaitForSingleObject(h, (uint)WaitResult.INFINITE) != WaitResult.WAIT_OBJECT_0)
                    throw new MemoryException("Failed to wait for remote thread");

                remoteThreads.Remove(threadId);

                uint exitCode;
                if (!Kernel32.GetExitCodeThread(h, out exitCode))
                    throw new MemoryException("Failed to obtain exit code");

                return exitCode;
            }
        }

        public uint Call(uint addr, CallingConventionEx cv, params object[] args)
        {
            using (var asm = new ManagedFasm())
            {
                asm.Clear();

                switch (cv)
                {
                    case CallingConventionEx.Cdecl:
                    {
                        asm.AddLine("push ebp");
                        for (var i = args.Length - 1; i >= 0; --i)
                            asm.AddLine("push {0}", args[i]);
                        asm.AddLine("mov eax, {0}", addr);
                        asm.AddLine("call eax");
                        for (var i = 0; i < args.Length; ++i)
                            asm.AddLine("pop ebp");
                        asm.AddLine("pop ebp");

                        asm.AddLine("retn");
                        break;
                    }
                    case CallingConventionEx.StdCall:
                    {
                        for (var i = args.Length - 1; i >= 0; --i)
                            asm.AddLine("push {0}", args[i]);
                        asm.AddLine("mov eax, {0}", addr);
                        asm.AddLine("call eax");
                        asm.AddLine("retn");
                        break;
                    }
                    case CallingConventionEx.FastCall:
                    {
                        if (args.Length > 0)
                            asm.AddLine("mov ecx, {0}", args[0]);
                        if (args.Length > 1)
                            asm.AddLine("mov edx, {0}", args[1]);
                        for (var i = args.Length - 1; i >= 2; --i)
                            asm.AddLine("push {0}", args[i]);
                        asm.AddLine("mov eax, {0}", addr);
                        asm.AddLine("call eax");
                        asm.AddLine("retn");
                        break;
                    }
                    case CallingConventionEx.Register:
                    {
                        if (args.Length > 0)
                            asm.AddLine("mov eax, {0}", args[0]);
                        if (args.Length > 1)
                            asm.AddLine("mov edx, {0}", args[1]);
                        if (args.Length > 2)
                            asm.AddLine("mov ecx, {0}", args[2]);
                        for (var i = 3; i < args.Length; ++i)
                            asm.AddLine("push {0}", args[i]);
                        asm.AddLine("mov ebx, {0}", addr);
                        asm.AddLine("call ebx");
                        asm.AddLine("retn");
                        break;
                    }
                    case CallingConventionEx.ThisCall:
                    {
                        if (args.Length > 0)
                            asm.AddLine("mov ecx, {0}", args[0]);
                        for (var i = args.Length - 1; i >= 1; --i)
                            asm.AddLine("push {0}", args[i]);
                        asm.AddLine("mov eax, {0}", addr);
                        asm.AddLine("call eax");
                        asm.AddLine("retn");
                        break;
                    }
                    default:
                    {
                        throw new MemoryException("Unhandled calling convention " + cv.ToString());
                    }
                }

                return ExecuteRemoteCode(asm.Assemble());
            }
        }

        public int GetThreadStartAddress(int threadId)
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
                    throw new MemoryException(string.Format("NtQueryInformationThread failed; NTSTATUS = {0:X8}", result));
                return BitConverter.ToInt32(buf, 0);
            }
            finally
            {
                Kernel32.CloseHandle(hThread);
            }
        }

        protected ModuleDump DumpModule(string name, bool refresh = false)
        {
            using (var suspender = Suspend())
            {
                if (refresh)
                    process.Refresh();

                var lowerName = name.ToLower();

                foreach (ProcessModule mod in process.Modules)
                {
                    if (mod.ModuleName.ToLower() == lowerName)
                    {
                        var dump = new ModuleDump(mod, this);
                        moduleDump[lowerName] = dump;
                        return dump;
                    }
                }
            }

            return null;
        }

        public ModuleDump GetModuleDump(string name, bool refresh = false)
        {
            var dumpColl = moduleDump.Where(d => d.Key == name.ToLower());
            if (dumpColl.Count() == 0 || refresh)
                return DumpModule(name, true);
            else
                return dumpColl.First().Value;
        }

        public uint Find(MemoryPattern pattern, string moduleName, int startAddress = 0, bool refresh = false)
        {
            var dump = GetModuleDump(moduleName, refresh);
            if (dump == null)
                return uint.MaxValue;

            return dump.Find(pattern, startAddress);
        }

        public uint FindNext(MemoryPattern pattern, string moduleName)
        {
            var dump = GetModuleDump(moduleName, false);
            if (dump == null)
                return uint.MaxValue;

            return dump.FindNext(pattern);
        }

        public Suspender Suspend()
        {
            return new Suspender(this);
        }
    }
}
