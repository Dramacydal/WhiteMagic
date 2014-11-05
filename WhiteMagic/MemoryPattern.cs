using System;
using System.Collections.Generic;
using System.Linq;

namespace WhiteMagic
{
    public class MemoryPattern
    {
        public class FindOptions
        {
            public int StartAddress = 0;
            public bool Reverse = false;
        }

        public uint Address { get { return address; } }
        public bool Found { get { return address != uint.MaxValue; } }

        protected uint address = uint.MaxValue;
        protected FindOptions findOptions = null;
        protected MemoryHandler m = null;
        protected string moduleName = null;

        public enum ValueType
        {
            Exact = 0,
            Any = 1,
        }

        public struct Element
        {
            public ValueType Type { get; set; }
            public byte Value { get; set; }
        }

        private Element[] Pattern;

        public MemoryPattern(byte[] pattern)
        {
            var pat = new List<Element>();

            foreach (var b in pattern)
                pat.Add(new Element() { Type = ValueType.Exact, Value = b });

            Pattern = pat.ToArray();
        }

        public MemoryPattern(string pattern)
        {
            var pat = new List<Element>();

            var tokens = pattern.Split(' ');
            foreach (var tok in tokens)
            {
                if (tok == string.Empty)
                    continue;

                Element elem;
                if (tok.Contains('?'))
                    elem = new Element()
                    {
                        Type = ValueType.Any,
                        Value = 0
                    };
                else
                    elem = new Element()
                    {
                        Type = ValueType.Exact,
                        Value = Convert.ToByte(tok, 16)

                    };

                pat.Add(elem);
            }

            Pattern = pat.ToArray();
        }

        public uint Find(MemoryHandler m, string moduleName, FindOptions findOptions = null, bool refreshMemory = false)
        {
            this.m = m;
            this.moduleName = moduleName;
            if (findOptions == null)
                findOptions = new FindOptions();
            this.findOptions = findOptions;

            var dump = m.GetModuleDump(moduleName, refreshMemory);
            if (dump == null)
                address = uint.MaxValue;
            else
                address = dump.FindPattern(this, findOptions);

            return address;
        }

        public uint FindNext()
        {
            if (address == uint.MaxValue)
                return address;

            var dump = m.GetModuleDump(moduleName, false);
            if (dump == null)
                return uint.MaxValue;

            findOptions.StartAddress = (int)address;

            if (findOptions.Reverse)
                --findOptions.StartAddress;
            else
                ++findOptions.StartAddress;

            address = dump.FindPattern(this, findOptions);
            return address;
        }

        public Element this[int index]
        {
            get { return Pattern[index]; }
        }

        public Element this[uint index]
        {
            get { return Pattern[index]; }
        }

        public int Length { get { return Pattern.Length; } }
    }
}
