using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using WhiteMagic;
using WhiteMagic.WinAPI;
using System.Runtime.InteropServices;
using WhiteMagic.Patterns;

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
            TestWOW();
            //Test2();
            //TestMemory();
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

        [StructLayout(LayoutKind.Sequential)]
        struct SMSGHandler
        {
            public IntPtr pName;            // 0x0
            public uint checker;            // 0x4
            public uint _2;                 // 0x8
            public uint _3;                 // 0xC
            public uint _4;                 // 0x10
            public uint dataHandler;        // 0x14
            public uint connectionChecker;  // 0x18
        }

        static void TestWOW()
        {
            var proc = MagicHelpers.FindProcessByInternalName("world of warcraft");
            if (proc == null)
            {
                Console.WriteLine("Process not found");
                return;
            }

            using (var m = new MemoryHandler(proc))
            {
                var v8 = IntPtr.Add(proc.MainModule.BaseAddress, 0x109A344 - 0x400000);
                var cnt = m.ReadUInt(IntPtr.Add(v8, 4));
                var start = m.ReadUInt(v8);
                var end = m.ReadUInt(v8) + cnt * 4;

                Console.WriteLine("cnt {0:X}", cnt);
                Console.WriteLine("start {0:X}", start - (int)proc.MainModule.BaseAddress + 0x400000);
                Console.WriteLine("end {0:X}", end - (int)proc.MainModule.BaseAddress + 0x400000);

                Console.WriteLine();

                for (var i = start; i < end; i += 4)
                {
                    var addr = new IntPtr(i);
                    var pHandler = m.Read<SMSGHandler>(m.ReadPointer(addr));
                    Console.WriteLine("'{0}'", m.ReadASCIIString(pHandler.pName));
                    Console.WriteLine("Checker: {0:X}", pHandler.checker - (int)proc.MainModule.BaseAddress + 0x400000);
                    Console.WriteLine("Connection Checker: {0:X}", pHandler.connectionChecker - (int)proc.MainModule.BaseAddress + 0x400000);
                    Console.WriteLine("DataHandler: {0:X}", pHandler.dataHandler - (int)proc.MainModule.BaseAddress + 0x400000);
                    Console.WriteLine("_2: {0:X}", pHandler._2);
                    Console.WriteLine("_3: {0:X}", pHandler._3);
                    Console.WriteLine("_4: {0:X}", pHandler._4 - (int)proc.MainModule.BaseAddress + 0x400000);
                    Console.WriteLine();
                }
            }
        }

        static void Test2()
        {
            var proc = MagicHelpers.FindProcessByName("notepad++.exe");
            if (proc == null)
            {
                Console.WriteLine("Failed to find process");
                return;
            }

            SystemInfo info;
            Kernel32.GetSystemInfo(out info);
            Console.WriteLine(info.ProcessorLevel);

            long MaxAddress = (long)info.MaximumApplicationAddress;
            long address = 0;
            do
            {
                MEMORY_BASIC_INFORMATION m;
                int result = Kernel32.VirtualQueryEx(proc.Handle, (IntPtr)address, out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                Console.WriteLine("0x{0:X}-0x{1:X} : 0x{2:X} bytes Flags: {3}", (uint)m.BaseAddress, (uint)m.BaseAddress + (uint)m.RegionSize - 1, (uint)m.RegionSize, m.AllocationProtect);
                if (address == (long)m.BaseAddress + (long)m.RegionSize)
                    break;
                address = (long)m.BaseAddress + (long)m.RegionSize;
            } while (address <= MaxAddress);
        }

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
            while (pat2.Offset != int.MaxValue)
            {
                Console.WriteLine("{0}", pat2.Offset);
                pat2.FindNext(bytes);
            }
            return;

            var proc = MagicHelpers.FindProcessByInternalName("world of warcraft");
            //var proc = Helpers.FindProcessByName("wow.exe");
            if (proc == null)
            {
                Console.WriteLine("Process not found");
                return;
            }

            if (!MagicHelpers.SetDebugPrivileges())
            {
                Console.WriteLine("Failed to set debug privileges");
                return;
            }

            // Console.WriteLine("{0:X}", addr - (int)proc.MainModule.BaseAddress + 0x400000);
            using (var m = new MemoryHandler(proc))
            {
                var pat = new MemoryPattern("0x11 0-3 0x22");
                m.Find(pat, proc.MainModule.ModuleName);
                while (pat.Offset != int.MaxValue)
                {
                    Console.WriteLine("{0}", pat.Offset);
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
