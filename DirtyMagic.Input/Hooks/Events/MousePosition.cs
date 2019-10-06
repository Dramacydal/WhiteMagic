using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirtyMagic.Hooks.Events
{
    public struct MousePosition
    {
        public MousePosition(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }
}
