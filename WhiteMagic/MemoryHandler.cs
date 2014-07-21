using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fasm;

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

    public class MemoryHandler
    {
        public Process Process { get { return process; } }
        public IntPtr ProcessHandle { get { return processHandle; } }
        public ManagedFasm Asm { get { return asm; } }

        protected IntPtr processHandle;
        protected Process process;

        protected ManagedFasm asm = new ManagedFasm();

        public MemoryHandler()
        {
        }

        ~MemoryHandler()
        {
            if (processHandle != IntPtr.Zero)
                WinApi.CloseHandle(processHandle);
        }

        public MemoryHandler(Process process)
        {
            SetProcess(process);
        }

        public void SetProcess(Process process)
        {
            this.process = process;
            if (processHandle != IntPtr.Zero)
                WinApi.CloseHandle(processHandle);

            processHandle = WinApi.OpenProcess(ProcessAccess.AllAccess, false, process.Id);
        }

        public bool IsValid()
        {
            if (process == null)
                return false;

            process.Refresh();

            return !process.HasExited;
        }

        public void SuspendAllThreads(int except = -1)
        {
            process.Refresh();

            foreach (ProcessThread pT in process.Threads)
            {
                if (except != -1 && pT.Id == except)
                    continue;

                IntPtr pOpenThread = WinApi.OpenThread(ThreadAccess.SUSPEND_RESUME, false, pT.Id);
                if (pOpenThread == IntPtr.Zero)
                    continue;

                WinApi.SuspendThread(pOpenThread);
                WinApi.CloseHandle(pOpenThread);
            }
        }

        public void ResumeAllThreads()
        {
            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = WinApi.OpenThread(ThreadAccess.SUSPEND_RESUME, false, pT.Id);
                if (pOpenThread == IntPtr.Zero)
                    continue;

                var suspendCount = 0;
                do
                {
                    suspendCount = WinApi.ResumeThread(pOpenThread);
                }
                while (suspendCount > 0);

                WinApi.CloseHandle(pOpenThread);
            }
        }

        #region Memory reading
        public byte[] ReadBytes(uint addr, int count)
        {
            var buf = new byte[count];

            PageProtection oldProtect, oldProtect2;
            if (!WinApi.VirtualProtectEx(processHandle, (IntPtr)addr, count, PageProtection.PAGE_EXECUTE_READWRITE, out oldProtect))
                throw new MemoryException("Failed to set page protection before read in remote process");

            int numBytes;
            if (!WinApi.ReadProcessMemory(processHandle, (IntPtr)addr, buf, count, out numBytes) || numBytes != count)
                throw new MemoryException("Failed to read memory in remote process");

            if (!WinApi.VirtualProtectEx(processHandle, (IntPtr)addr, count, oldProtect, out oldProtect2))
                throw new MemoryException("Failed to set page protection after read in remote process");

            return buf;
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

        protected byte[] ReadNullTerminatedBytes(uint addr)
        {
            var bytes = new List<byte>();
            for (; ; )
            {
                var b = ReadByte(addr++);
                bytes.Add(b);
                if (b == 0)
                    break;
            }

            return bytes.ToArray();
        }

        public string ReadCString(uint addr, int len = 0)
        {
            return Encoding.ASCII.GetString(len == 0 ? ReadNullTerminatedBytes(addr) : ReadBytes(addr, len));
        }

        public string ReadUTF8String(uint addr, int len = 0)
        {
            return Encoding.UTF8.GetString(len == 0 ? ReadNullTerminatedBytes(addr) : ReadBytes(addr, len));
        }

        public string ReadUTF16String(uint addr, int len = 0)
        {
            return Encoding.Unicode.GetString(len == 0 ? ReadNullTerminatedBytes(addr) : ReadBytes(addr, len));
        }

        public string ReadUTF32String(uint addr, int len = 0)
        {
            if (len == 0)
                return Encoding.UTF32.GetString(ReadNullTerminatedBytes(addr));

            return Encoding.UTF32.GetString(ReadBytes(addr, len));
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
            PageProtection oldProtect, oldProtect2;
            if (!WinApi.VirtualProtectEx(processHandle, (IntPtr)addr, bytes.Length, PageProtection.PAGE_EXECUTE_READWRITE, out oldProtect))
                throw new MemoryException("Failed to set page protection before write in remote process");

            int numBytes;
            if (!WinApi.WriteProcessMemory(processHandle, (IntPtr)addr, bytes, bytes.Length, out numBytes) || numBytes != bytes.Length)
                throw new MemoryException("Failed to write memory in remote process");

            if (!WinApi.VirtualProtectEx(processHandle, (IntPtr)addr, bytes.Length, oldProtect, out oldProtect2))
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
            var addr = WinApi.VirtualAllocEx(processHandle, IntPtr.Zero, size, AllocationType.Commit | AllocationType.Reserve, PageProtection.PAGE_EXECUTE_READWRITE);
            if (addr == 0)
                throw new MemoryException("Failed to allocate memory in remote process");

            return addr;
        }

        public void FreeMemory(uint addr)
        {
            if (!WinApi.VirtualFreeEx(processHandle, (IntPtr)addr, 0, FreeType.Release))
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
            var addr = AllocateMemory(bytes.Length);
            WriteBytes(addr, bytes);

            var exitCode = ExecuteRemoteCode(addr);
            FreeMemory(addr);

            return exitCode;
        }

        public uint ExecuteRemoteCode(uint addr)
        {
            int threadId;
            var h = WinApi.CreateRemoteThread(processHandle, IntPtr.Zero, 0, addr, IntPtr.Zero, 0, out threadId);
            if (h == IntPtr.Zero)
                throw new MemoryException("Failed to create remote thread");

            if (WinApi.WaitForSingleObject(h, WinApi.INFINITE) != WaitResult.WAIT_OBJECT_0)
                throw new MemoryException("Failed to wait for remote thread");

            uint exitCode;
            if (!WinApi.GetExitCodeThread(h, out exitCode))
                throw new MemoryException("Failed to obtain exit code");

            return exitCode;
        }

        public uint Call(uint addr, CallingConventionEx cv, params uint[] args)
        {
            asm.Clear();

            switch (cv)
            {
                case CallingConventionEx.Cdecl:
                {
                    for (var i = args.Length - 1; i >= 0; --i)
                        asm.AddLine("push {0}", args[i]);
                    asm.AddLine("mov eax, {0}", addr);
                    asm.AddLine("call eax");
                    if (args.Length != 0)
                        asm.AddLine("retn {0}", 4 * args.Length);
                    else
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
}
