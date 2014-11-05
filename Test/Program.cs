using System;
using System.Text;
using WhiteMagic;

namespace Test
{
    public static class Extensions
    {
        public static long MSecToNow(this DateTime date)
        {
            return (DateTime.Now.Ticks - date.Ticks) / TimeSpan.TicksPerMillisecond;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //TestBreakPoints();
            TestMemory();
            //TestThreads();
            //DelegateTest();
        }

        static string[] bases = new string[]
        {
            "CGObjectData::m_guid",
            "CGUnitData::charm",
            "CGItemData::m_owner",

        };

        static string[] bases2 = new string[]
        {
            "CGObjectData::",
            "CGUnitData::",
            "CGItemData::",
        };

        static void TestMemory()
        {
            //var proc = Helpers.FindProcessByInternalName("world of warcraft");
            var proc = Helpers.FindProcessByName("uwow.exe");
            if (proc == null)
            {
                Console.WriteLine("Process not found");
                return;
            }

            if (!WinApi.SetDebugPrivileges())
            {
                Console.WriteLine("Failed to set debug privileges");
                return;
            }

            // Console.WriteLine("{0:X}", addr - (int)proc.MainModule.BaseAddress + 0x400000);
            using (var m = new MemoryHandler(proc))
            {
                var pattern = new MemoryPattern(Encoding.ASCII.GetBytes("CGObjectData::"));
                pattern.Find(m, proc.MainModule.ModuleName);
                while (pattern.Found)
                {
                    Console.WriteLine("{0:X}", pattern.Address - (int)proc.MainModule.BaseAddress + 0x400000);
                    pattern.FindNext();
                }

                Console.WriteLine();

                pattern = new MemoryPattern(Encoding.ASCII.GetBytes("CGObjectData::"));
                if (pattern.Find(m, proc.MainModule.ModuleName, new MemoryPattern.FindOptions() { Reverse = true }) != uint.MaxValue)
                {
                    do
                    {
                        Console.WriteLine("{0:X}", pattern.Address - (int)proc.MainModule.BaseAddress + 0x400000);
                    }
                    while (pattern.FindNext() != uint.MaxValue);
                }

                /*foreach (var str in bases)
                {
                    Console.WriteLine(str);
                    var addr = m.FindPattern("wow.exe", new BytePattern(Encoding.ASCII.GetBytes(str)));
                    if (addr != uint.MaxValue)
                    {
                        addr = m.FindPattern("wow.exe", new BytePattern("C7 ?? ?? ?? ?? ?? " + BitConverter.GetBytes(addr).AsHexString()));
                        if (addr != uint.MaxValue)
                        {
                            addr = m.ReadUInt(addr + 2);
                            Console.WriteLine("{0:X}", addr - (int)proc.MainModule.BaseAddress + 0x400000);
                            continue;
                        }
                    }
                    Console.WriteLine("Fail");
                }*/
            }
        }
    }
}
