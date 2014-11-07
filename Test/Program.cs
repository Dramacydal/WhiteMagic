using System;
using System.Text;
using System.Linq;
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

        static string[] descriptorTypes = new string[]
        {
            "CGUnitDynamicData::",
            "CGObjectData::",
            "CGUnitData::",
            "CGPlayerData::",
            "CGItemData::",
            "CGContainerData::",
            "CGGameObjectData::",
            "CGDynamicObjectData::",
            "CGCorpseData::",
            "CGAreaTriggerData::",
            "CGSceneObjectData::",
            "CGConversationData::"
        };

        static string[] descriptorDynamicTypes = new string[]
        {
            "CGUnitDynamicData::",
            "CGPlayerDynamicData::",
            "CGItemDynamicData::",
            "CGConversationDynamicData::"
        };

        static void TestMemory()
        {
            var bytes = new byte[]
                {
                    0x11, 0x22,
                    0x33,
                    0x11, 0x00, 0x22,
                    0x33,
                    0x11, 0x00, 0x00, 0x22,
                    0x33,
                    0x11, 0x00, 0x00, 0x00, 0x22,
                    0x44
                };

            var pat2 = new MemoryPattern("0x11 2-3 0x22");
            pat2.Find(bytes);
            while (pat2.Address != uint.MaxValue)
            {
                Console.WriteLine("{0}", pat2.Address);
                pat2.FindNext(bytes);
            }
            return;

            var proc = Helpers.FindProcessByInternalName("world of warcraft");
            //var proc = Helpers.FindProcessByName("wow.exe");
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
                var pat = new MemoryPattern("0x11 0-3 0x22");
                m.Find(pat, proc.MainModule.ModuleName);
                while (pat.Address != uint.MaxValue)
                {
                    Console.WriteLine("{0}", pat.Address);
                    m.FindNext(pat, proc.MainModule.ModuleName);
                }

                return;

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
