using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace WhiteMagic.Patterns
{
    public class MemoryPattern : IEnumerable
    {
        public int Offset { get { return offset; } }
        public bool Found { get { return offset != int.MaxValue; } }

        protected int offset = int.MaxValue;

        public Element[] Pattern { get; private set; }

        private MemoryPattern(Element[] elements)
        {
            Pattern = elements;
        }

        public MemoryPattern(byte[] pattern)
        {
            Pattern = pattern.Select(e => new Element(e)).ToArray();
        }

        public MemoryPattern(string pattern)
        {
            var tokens = Regex.Replace(pattern, @"\s+", " ").Trim(' ').Split(' ');
            try
            {
                Pattern = tokens.Select(e => new Element(e)).ToArray();
            }
            catch (Exception e)
            {
                throw new PatternException("Wrong pattern format: " + e.Message);
            }
        }

        public int Find(byte[] bytes, int startOffset = 0)
        {
            offset = _Find(bytes, startOffset);
            return offset;
        }

        protected int _Find(byte[] bytes, int startOffset = 0)
        {
            if (startOffset >= bytes.Length)
                return offset;

            if (Length == 0)
                return startOffset;

            for (var curAddr = startOffset; curAddr < bytes.Length; ++curAddr)
            {
                if (curAddr + Length > bytes.Length)
                    return int.MaxValue;

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

                    if (e.Type == ValueType.AnySequence)
                    {
                        if (j == Pattern.Length - 1)
                            return int.MaxValue;

                        var seqMatches = false;
                        var seqLength = 0;
                        for (; seqLength <= e.MaxLength; ++seqLength)
                        {
                            if (curAddr + 1 + seqLength >= bytes.Length)
                                return int.MaxValue;

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
                    return curAddr;
            }

            return int.MaxValue;
        }

        public int FindNext(byte[] bytes)
        {
            return Find(bytes, offset + 1);
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

        public override string ToString()
        {
            return string.Join(" ", Pattern.Select(it =>
            {
                switch (it.Type)
                {
                    case ValueType.Equal:
                        return string.Format("{0:X2}", it.Value);
                    case ValueType.Any:
                        return "??";
                    default:
                        break;
                }
                return "XX";
            }));
        }
    }
}
