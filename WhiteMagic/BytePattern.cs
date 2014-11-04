using System;
using System.Collections.Generic;
using System.Linq;

namespace WhiteMagic
{
    public class BytePattern
    {
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

        public BytePattern(byte[] pattern)
        {
            var pat = new List<Element>();

            foreach (var b in pattern)
                pat.Add(new Element() { Type = ValueType.Exact, Value = b });

            Pattern = pat.ToArray();
        }

        public BytePattern(string pattern)
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
