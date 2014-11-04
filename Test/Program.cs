using System;
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

        static void TestMemory()
        {
            var proc = Helpers.FindProcessByInternalName("world of warcraft");
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

            using (var m = new MemoryHandler(proc))
            {
                var addr = 0u;
                for (; addr != uint.MaxValue; )
                {
                    addr = m.FindPattern("wow.exe",
                        new BytePattern("55 8b ec ff 05 ?? ?? ?? ?? 56 8b 75 10 57 8d 45 10 8b f9 50 8b ce e8"),
                        addr + 1);
                    Console.WriteLine("{0:X}", addr - (int)proc.MainModule.BaseAddress + 0x400000);
                }

                addr = 0;
                for (; addr != uint.MaxValue; )
                {
                    addr = m.FindPattern("wow.exe",
                        new BytePattern("55 8b ec 83 ec 10 53 56 8b f1 8d ?? ?? ?? ?? ?? 57 89 ?? ?? e8 ?? ?? ?? ?? 83 ?? ?? ?? ?? ?? 05"),
                        addr + 1);
                    Console.WriteLine("{0:X}", addr - (int)proc.MainModule.BaseAddress + 0x400000);
                }
            }
        }
    }
}
