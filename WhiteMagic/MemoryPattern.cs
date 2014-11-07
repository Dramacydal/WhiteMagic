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

        public enum ValueType
        {
            Equal = 0,
            Greater = 1,
            Less = 2,
            Any = 3,
            Mask = 4,
            AnySequence = 5,
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
                    case ValueType.Equal:
                        return Value == value;
                    case ValueType.Greater:
                        return value > Value;
                    case ValueType.Less:
                        return value < Value;
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
                pat.Add(new Element() { Type = ValueType.Equal, Value = b });

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
                    else if (tok.Contains('>'))
                    {
                        elem = new Element()
                        {
                            Type = ValueType.Greater,
                            Value = Convert.ToByte(tok.Replace(">", ""), 16),
                        };
                    }
                    else if (tok.Contains('<'))
                    {
                        elem = new Element()
                        {
                            Type = ValueType.Less,
                            Value = Convert.ToByte(tok.Replace("<", ""), 16),
                        };
                    }
                    else if (tok.Contains("-"))
                    {
                        var t = tok.Split('-');

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
                            Type = ValueType.Equal,
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

        public uint Find(byte[] bytes, int startAddress = 0)
        {
            address = _Find(bytes, startAddress);
            return address;
        }

        protected uint _Find(byte[] bytes, int startAddress = 0)
        {
            if (startAddress >= bytes.Length)
                return address;

            if (Length == 0)
                return (uint)startAddress;

            for (var curAddr = startAddress; curAddr < bytes.Length; ++curAddr)
            {
                if (curAddr + Length > bytes.Length)
                    return uint.MaxValue;

                var match = true;
                var offs = 0;
                for (var j = 0; j < Length; ++j)
                {
                    var e = Pattern[j];
                    if (!e.Matches(bytes[curAddr + offs]))
                    {
                        match = false;
                        break;
                    }

                    if (e.Type == MemoryPattern.ValueType.AnySequence)
                    {
                        if (j == Pattern.Length - 1)
                            return (uint)(curAddr);

                        var seqMatches = false;
                        var seqLength = 0;
                        for (; seqLength <= e.MaxLength; ++seqLength)
                        {
                            if (curAddr + 1 + seqLength >= bytes.Length)
                                return uint.MaxValue;

                            if (Pattern[j + 1].Matches(bytes[curAddr + 1 + seqLength]))
                            {
                                if (seqLength >= e.MinLength)
                                {
                                    seqMatches = true;
                                    break;
                                }
                            }
                        }

                        if (!seqMatches)
                        {
                            match = false;
                            break;
                        }

                        offs += seqLength;
                    }
                    else
                        ++offs;
                }

                if (match)
                    return (uint)(curAddr);
            }

            return uint.MaxValue;
        }

        public uint FindNext(byte[] bytes)
        {
            return Find(bytes, (int)Address + 1);
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
