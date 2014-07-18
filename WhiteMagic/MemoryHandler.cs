using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteMagic
{
    public class MemoryException : Exception
    {
        public MemoryException(string message) : base(message) { }
    }

    public class MemoryHandler
    {
        public Process Process { get { return process; } }

        protected Process process;

        public MemoryHandler() { }

        public MemoryHandler(Process process)
        {
            SetProcess(process);
        }

        ~MemoryHandler()
        {
        }

        public void SetProcess(Process process)
        {
            this.process = process;
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

        public byte[] ReadBytes(uint addr, int count)
        {
            var buf = new byte[count];

            PageProtection oldProtect, oldProtect2;
            if (!WinApi.VirtualProtectEx(process.Handle, (IntPtr)addr, count, PageProtection.PAGE_EXECUTE_READWRITE, out oldProtect))
                throw new MemoryException("Failed to set page protection before read");

            int numBytes;
            if (!WinApi.ReadProcessMemory(process.Handle, (IntPtr)addr, buf, count, out numBytes) || numBytes != count)
                throw new MemoryException("Failed to read memory");

            if (!WinApi.VirtualProtectEx(process.Handle, (IntPtr)addr, count, oldProtect, out oldProtect2))
                throw new MemoryException("Failed to set page protection after read");

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
            if (len == 0)
                return Encoding.ASCII.GetString(ReadNullTerminatedBytes(addr));

            return Encoding.ASCII.GetString(ReadBytes(addr, len));
        }

        public string ReadUTF8String(uint addr, int len = 0)
        {
            if (len == 0)
                return Encoding.UTF8.GetString(ReadNullTerminatedBytes(addr));

            return Encoding.UTF8.GetString(ReadBytes(addr, len));
        }

        public string ReadUTF16String(uint addr, int len = 0)
        {
            if (len == 0)
                return Encoding.Unicode.GetString(ReadNullTerminatedBytes(addr));

            return Encoding.Unicode.GetString(ReadBytes(addr, len));
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
    }
}
