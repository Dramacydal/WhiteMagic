using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DirtyMagic.Patterns;

namespace DirtyMagic
{
    public class MemoryDump
    {
        private const int readCount = 256;

        public MemoryDump(IntPtr StartAddress, long Length)
        {
            this.StartAddress = StartAddress;
            this.Length = Length;
        }

        public MemoryDump(IntPtr StartAddress, byte[] Data)
            : this(StartAddress, Data.LongLength)
        {
            this.Data = Data;
        }

        public MemoryDump(MemoryHandler Memory, IntPtr Address, long Length)
            : this(Address, Length)
        {
            Read(Memory);
        }

        public void Read(MemoryHandler Memory)
        {
            var bytes = new List<byte>();
            for (long i = 0; i < Length; i += readCount)
                bytes.AddRange(Memory.ReadBytes(IntPtr.Add(StartAddress, (int)i), i + readCount >= Length ? (int)(Length - i) : readCount));

            Data = bytes.ToArray();
        }

        public IntPtr StartAddress { get; }
        public long Length { get; }

        private byte[] _data { get; set; } = null;
        public byte[] Data
        {
            get { return _data; }
            private set { _data = value; StringDump = PatternHelper.BytesToString(_data); }
        }
        protected string StringDump { get; private set; }

        public int Size => Data.Length;

        public Match Match(MemoryPattern Pattern) => Pattern.Match(StringDump);

        public MatchCollection Matches(MemoryPattern Pattern) => Pattern.Matches(StringDump);
    }
}
