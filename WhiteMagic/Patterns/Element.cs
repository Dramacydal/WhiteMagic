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

        public Element(string token)
        {
            if (token == "??")
            {
                Type = ValueType.Any;
                Value = 0;
            }
            else if (token.Contains('m'))
            {
                Type = ValueType.Mask;
                Value = Convert.ToByte(token.Replace("m", ""), 16);
            }
            else if (token.Contains('>'))
            {
                Type = ValueType.Greater;
                Value = Convert.ToByte(token.Replace(">", ""), 16);
            }
            else if (token.Contains('<'))
            {
                Type = ValueType.Less;
                Value = Convert.ToByte(token.Replace("<", ""), 16);
            }
            else if (token.Contains("-"))
            {
                var t = token.Split('-');

                Type = ValueType.AnySequence;
                MinLength = Convert.ToByte(t[0]);
                MaxLength = Convert.ToByte(t[1]);
            }
            else
            {
                Type = ValueType.Equal;
                Value = Convert.ToByte(token, 16);
            }
        }
    }
}
