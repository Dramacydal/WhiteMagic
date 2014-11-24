using System;
using System.Linq;

namespace WhiteMagic.Patterns
{
    public class Element
    {
        public ValueType Type { get; private set; }

        public byte Value { get; private set; }
        public byte MinLength { get; private set; }
        public byte MaxLength { get; private set; }

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

        public Element(byte val)
        {
            Type = ValueType.Equal;
            Value = val;
        }

        public Element(string tok)
        {
            if (tok == "??")
            {
                Type = ValueType.Any;
                Value = 0;
            }
            else if (tok.Contains('m'))
            {
                Type = ValueType.Mask;
                Value = Convert.ToByte(tok.Replace("m", ""), 16);
            }
            else if (tok.Contains('>'))
            {
                Type = ValueType.Greater;
                Value = Convert.ToByte(tok.Replace(">", ""), 16);
            }
            else if (tok.Contains('<'))
            {
                Type = ValueType.Less;
                Value = Convert.ToByte(tok.Replace("<", ""), 16);
            }
            else if (tok.Contains("-"))
            {
                var t = tok.Split('-');

                Type = ValueType.AnySequence;
                MinLength = Convert.ToByte(t[0]);
                MaxLength = Convert.ToByte(t[1]);
            }
            else
            {
                Type = ValueType.Equal;
                Value = Convert.ToByte(tok, 16);
            }
        }
    }
}
