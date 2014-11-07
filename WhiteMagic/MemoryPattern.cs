using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WhiteMagic
{
    public class PatternException : Exception
    {
        public PatternException(string message)
            : base(message) { }
    }

    public class MemoryPattern : IEnumerable
    {
        public uint Address { get { return address; } }
        public bool Found { get { return address != uint.MaxValue; } }

        protected uint address = uint.MaxValue;
        protected int startAddress = 0;
        protected bool reverse = false;
        protected MemoryHandler m = null;
        protected string moduleName = null;

        public enum ValueType
        {
            Exact = 0,
            Any = 1,
            Mask = 2,
            AnySequence = 3
        }

        public class Element
        {
            public ValueType Type { get; set; }

            public byte Value { get; set; }
            public byte MinLength { get; set; }
            public byte MaxLength { get; set; }

            public bool Matches(byte value)
            {
                switch (Type)
                {
                    case ValueType.Exact:
                        return Value == value;
                    case ValueType.Any:
                        return true;
                    case ValueType.Mask:
                        return (Value & value) != 0;
                    case ValueType.AnySequence:
                        return true;
                    default:
                        break;
                }

                return false;
            }
        }

        private Element[] Pattern;

        private MemoryPattern(Element[] elements)
        {
            Pattern = elements;
        }

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
            try
            {
                foreach (var tok in tokens)
                {
                    if (tok == string.Empty)
                        continue;

                    Element elem;
                    if (tok == "??")
                    {
                        elem = new Element()
                        {
                            Type = ValueType.Any,
                            Value = 0
                        };
                    }
                    else if (tok.Contains('m'))
                    {
                        elem = new Element()
                        {
                            Type = ValueType.Mask,
                            Value = Convert.ToByte(tok.Replace("m", ""), 16),
                        };
                    }
                    else if (tok.Contains("-"))
                    {
                        var t = tok.Replace("L", "").Split('-');

                        elem = new Element()
                        {
                            Type = ValueType.AnySequence,
                            MinLength = Convert.ToByte(t[0]),
                            MaxLength = Convert.ToByte(t[1]),
                        };
                    }
                    else
                    {
                        elem = new Element()
                        {
                            Type = ValueType.Exact,
                            Value = Convert.ToByte(tok, 16)

                        };
                    }

                    pat.Add(elem);
                }
            }
            catch(Exception e)
            {
                throw new PatternException("Wrong pattern format: " + e.Message);
            }

            Pattern = pat.ToArray();
        }

        public uint Find(MemoryHandler m, string moduleName, int startAddress = 0, bool reverse = false, bool refreshMemory = false)
        {
            this.m = m;
            this.moduleName = moduleName;
            this.startAddress = startAddress;
            this.reverse = reverse;

            var dump = m.GetModuleDump(moduleName, refreshMemory);
            if (dump == null)
                address = uint.MaxValue;
            else
                address = dump.FindPattern(this, startAddress, reverse);

            return address;
        }

        public uint FindNext()
        {
            if (address == uint.MaxValue)
                return address;

            var dump = m.GetModuleDump(moduleName, false);
            if (dump == null)
                return uint.MaxValue;

            startAddress = (int)address;

            if (reverse)
                --startAddress;
            else
                ++startAddress;

            address = dump.FindPattern(this, startAddress, reverse);
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

        public IEnumerator GetEnumerator()
        {
            foreach (var e in Pattern)
                yield return e;
        }

        public MemoryPattern Skip(int count)
        {
            return new MemoryPattern(Pattern.Skip(count).ToArray());
        }
    }
}
