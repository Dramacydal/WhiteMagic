using System;
using System.Text.RegularExpressions;
using WhiteMagic.Patterns;

namespace WhiteMagic
{
    public class MemoryContainer
    {
        public MemoryContainer(IntPtr BaseAddress)
        {
            this.BaseAddress = BaseAddress;
        }

        public MemoryContainer(byte[] Data, IntPtr BaseAddress)
            : this(BaseAddress)
        {
            this.Data = Data;
        }

        public MemoryContainer(MemoryHandler m, IntPtr Address, int Length)
            : this(Address)
        {
            this.Data = m.ReadBytes(Address, Length);
        }

        public IntPtr BaseAddress { get; private set; }

        private byte[] _data { get; set; }
        public byte[] Data
        {
            get { return _data; }
            protected set { _data = value; StringDump = PatternHelper.BytesToString(_data); }
        }
        protected string StringDump { get; private set; }

        public int DataLength { get { return Data.Length; } }

        public Match Match(MemoryPattern Pattern)
        {
            return Pattern.Match(StringDump);
        }

        public MatchCollection Matches(MemoryPattern Pattern)
        {
            return Pattern.Matches(StringDump);
        }
    }
}
