using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DirtyMagic.Patterns;

namespace DirtyMagic
{
    public class MemoryDump
    {
        private const int readCount = 256;

        public MemoryDump(IntPtr startAddress, long length)
        {
            this.StartAddress = startAddress;
            this.Length = length;
        }

        public MemoryDump(IntPtr startAddress, byte[] data)
            : this(startAddress, data.LongLength)
        {
            this.Data = data;
        }

        public MemoryDump(MemoryHandler memory, IntPtr address, long length)
            : this(address, length)
        {
            Read(memory);
        }

        public void Read(MemoryHandler memory)
        {
            var bytes = new List<byte>();
            for (long i = 0; i < Length; i += readCount)
                bytes.AddRange(memory.ReadBytes(IntPtr.Add(StartAddress, (int)i), i + readCount >= Length ? (int)(Length - i) : readCount));

            Data = bytes.ToArray();
        }

        public IntPtr StartAddress { get; }
        public long Length { get; }

        private byte[] _data = null;
        public byte[] Data
        {
            get => _data;
            private set { _data = value; StringDump = PatternHelper.BytesToString(_data); }
        }
        protected string StringDump { get; private set; }

        public int Size => Data.Length;

        public Match Match(MemoryPattern pattern) => pattern.Match(StringDump);

        public MatchCollection Matches(MemoryPattern pattern) => pattern.Matches(StringDump);
    }
}
