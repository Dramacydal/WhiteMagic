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

        #region Memory reading
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
            if (!WinApi.VirtualProtectEx(process.Handle, (IntPtr)addr, bytes.Length, PageProtection.PAGE_EXECUTE_READWRITE, out oldProtect))
                throw new MemoryException("Failed to set page protection before write");

            int numBytes;
            if (!WinApi.WriteProcessMemory(process.Handle, (IntPtr)addr, bytes, bytes.Length, out numBytes) || numBytes != bytes.Length)
                throw new MemoryException("Failed to write memory");

            if (!WinApi.VirtualProtectEx(process.Handle, (IntPtr)addr, bytes.Length, oldProtect, out oldProtect2))
                throw new MemoryException("Failed to set page protection after write");
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
    }
}
