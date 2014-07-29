using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using WhiteMagic;
using Fasm;
using CONTEXT = WhiteMagic.CONTEXT;

namespace Test
{
    class Program
    {
        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Catched Ctrl+C");
            args.Cancel = true;
            pd.StopDebugging();
        }

        public class IncBreakPoint : HardwareBreakPoint
        {
            public IncBreakPoint(int address, uint len, Condition condition) : base(address, len, condition) { }

            public override bool HandleException(ref CONTEXT ctx, ProcessDebugger pd)
            {
                // skip 'inc a'
                ctx.Eip += 6;

                uint addr = 0x404430 - 0x400000 + pd.GetModuleAddress("program.exe");
                pd.WriteUInt(addr,
                    pd.ReadUInt(addr) + 10);

                return true;
            }
        }

        public static ProcessDebugger pd;

        static void Main(string[] args)
        {
            //TestBreakPoints();
            TestMemory();
            //TestThreads();
            //DelegateTest();
        }

        static void DelegateTest()
        {
            var f = new MyFunc(Asd);

            f();
        }

        public delegate void MyFunc();

        static void Asd()
        {
            Console.WriteLine("123");
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct UnitAny
        {
            [FieldOffset(0x0)]
            public uint dwType;
            [FieldOffset(0x4)]
            public uint dwTxtFileNo;
            [FieldOffset(0x8)]
            public uint _1;
            [FieldOffset(0xC)]
            public uint dwUnitId;
            [FieldOffset(0x10)]
            public uint dwMode;
            [FieldOffset(0x14)]
            public uint pPlayerData;        // PlayerData*
            [FieldOffset(0x14)]
            public uint pItemData;          // ItemData*
            [FieldOffset(0x14)]
            public uint pMonsterData;       // MonsterData*
            [FieldOffset(0x14)]
            public uint pObjectData;        // ObjectData*
            [FieldOffset(0x18)]
            public uint dwAct;
            [FieldOffset(0x1C)]
            public uint pAct;               // Act*
            [FieldOffset(0x20)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
            public uint[] dwSeed;
            [FieldOffset(0x28)]
            public uint _2;
            [FieldOffset(0x2C)]
            public uint pPath;              // Path*
            [FieldOffset(0x2C)]
            public uint pItemPath;          // ItemPath*
            [FieldOffset(0x2C)]
            public uint pObjectPath;        // ObjectPath*
            [FieldOffset(0x30)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
            public uint[] _3;
            [FieldOffset(0x44)]
            public uint dwGfxFrame;
            [FieldOffset(0x48)]
            public uint dwFrameRemain;
            [FieldOffset(0x4C)]
            public ushort wFrameRate;
            [FieldOffset(0x4E)]
            public ushort _4;
            [FieldOffset(0x50)]
            public uint pGfxUnk;            // BYTE*
            [FieldOffset(0x54)]
            public uint pGfxInfo;           // DWORD*
            [FieldOffset(0x58)]
            public uint _5;
            [FieldOffset(0x5C)]
            public uint pStats;             // StatList*
            [FieldOffset(0x60)]
            public uint pInventory;         // Inventory*
            [FieldOffset(0x64)]
            public uint ptLight;            // Light*
            [FieldOffset(0x68)]
            public uint dwStartLightRadius;
            [FieldOffset(0x6C)]
            public ushort nPl2ShiftIdx;
            [FieldOffset(0x6E)]
            public ushort nUpdateType;
            [FieldOffset(0x70)]
            public uint pUpdateUnit;        // UnitAny* - Used when updating unit.
            [FieldOffset(0x74)]
            public uint pQuestRecord;       // DWORD*
            [FieldOffset(0x78)]
            public uint bSparklyChest;      // bool
            [FieldOffset(0x7C)]
            public uint pTimerArgs;         // DWORD*
            [FieldOffset(0x80)]
            public uint dwSoundSync;
            [FieldOffset(0x84)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
            public uint[] _6;
            [FieldOffset(0x8C)]
            public ushort wX;
            [FieldOffset(0x8E)]
            public ushort wY;
            [FieldOffset(0x90)]
            public uint _7;
            [FieldOffset(0x94)]
            public uint dwOwnerType;
            [FieldOffset(0x98)]
            public uint dwOwnerId;
            [FieldOffset(0x9C)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
            public uint[] _8;
            [FieldOffset(0xA4)]
            public uint pOMsg;              // OverheadMsg*
            [FieldOffset(0xA8)]
            public uint pInfo;              // Info*
            [FieldOffset(0xAC)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.U4)]
            public uint[] _9;
            [FieldOffset(0xC4)]
            public uint dwFlags;
            [FieldOffset(0xC8)]
            public uint dwFlags2;
            [FieldOffset(0xCC)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
            public uint[] _10;
            [FieldOffset(0xE0)]
            public uint pChangedNext;       // UnitAny*
            [FieldOffset(0xE4)]
            public uint pListNext;          // UnitAny* 0xE4 -> 0xD8
            [FieldOffset(0xE8)]
            public uint pRoomNext;          // UnitAny*
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Inventory
        {
            [FieldOffset(0x0)]
            public uint dwSignature;
            [FieldOffset(0x04)]
            public uint bGame1C;            // BYTE*
            [FieldOffset(0x08)]
            public uint pOwner;             // UnitAny*
            [FieldOffset(0x0C)]
            public uint pFirstItem;         // UnitAny*
            [FieldOffset(0x10)]
            public uint pLastItem;          // UnitAny*
            [FieldOffset(0x14)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
            public uint[] _1;
            [FieldOffset(0x1C)]
            public uint dwLeftItemUid;
            [FieldOffset(0x20)]
            public uint pCursorItem;        // UnitAny*
            [FieldOffset(0x24)]
            public uint dwOwnerId;
            [FieldOffset(0x28)]
            public uint dwItemCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ItemData
        {
            public uint dwQuality;				//0x00
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
            public uint[] _1;					//0x04
            public uint dwItemFlags;				//0x0C 1 = Owned by player, 0xFFFFFFFF = Not owned
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
            public uint[] _2;					//0x10
            public uint dwFlags;					//0x18
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.U4)]
            public uint[] _3;					//0x1C
            public uint dwQuality2;				//0x28
            public uint dwItemLevel;				//0x2C
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
            public uint[] _4;					//0x30
            public ushort wPrefix;					//0x38
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U2)]
            public ushort[] _5;						//0x3A
            public ushort wSuffix;					//0x3E
            public uint _6;						//0x40
            public byte BodyLocation;				//0x44
            public byte ItemLocation;				//0x45 Non-body/belt location (Body/Belt == 0xFF)
            public byte _7;						//0x46
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x15, ArraySubType = UnmanagedType.U2)]
            public byte[] _8;
            public uint pOwnerInventory;		// Inventory 0x5C +
            public uint _10;						//0x60
            public uint pNextInvItem;			// UnitAny 0x64
            public byte GameLocation;			//0x68
            public byte NodePage;					//0x69 Actual location, this is the most reliable by far
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x9A, ArraySubType = UnmanagedType.U1)]
            public byte[] _12;						//0x6A
            public uint pOwner;				// UnitAny 0x84 0x104
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ItemData2
        {
            [FieldOffset(0x2C)]
            public uint dwItemLevel;
            [FieldOffset(0x70)]
            public uint pNextInvItem;
            [FieldOffset(0x104)]
            public uint pOwner;
        }

        static void TestThreads()
        {
            Process proc = null;
            var processes = Process.GetProcessesByName("ThreadTest2");
            if (processes.Length == 0)
            {
                Console.WriteLine("Could not find process");
                return;
            }

            proc = processes[0];

            if (!WinApi.SetDebugPrivileges())
            {
                Console.WriteLine("Failed to set debug privileges");
                return;
            }

            Console.WriteLine("Threads count: {0}", proc.Threads.Count);

            var m = new MemoryHandler(proc);
            m.SuspendAllThreads();
            m.Call(0x4155B0 - 0x400000 + (uint)proc.MainModule.BaseAddress,
                CallingConventionEx.StdCall, 999999);
            m.ResumeAllThreads();
        }

        static void TestMemory()
        {
            Process proc = null;
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                try
                {
                    if (process.MainModule.FileVersionInfo.InternalName.ToLower().Contains("diablo ii"))
                        proc = process;
                }
                catch (NullReferenceException)
                {
                }
                catch (Win32Exception)
                {
                }
            }

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

            var m = new MemoryHandler(proc);

            m.SuspendAllThreads();

            var w = new StreamWriter(@"d:\d2work\codes.txt");
            try
            {
                uint pMaxItem = 0x6FDF4CB0;
                uint pData = 0x6FDF4CB4;
                uint getTxt = 0x62C70 + 0x6FD50000;

                var maxItem = m.ReadUInt(pMaxItem);
                Console.WriteLine("Max item: " + maxItem.ToString());
                for (uint i = 0; i <= maxItem; ++i)
                {
                    var pText = m.Call(getTxt, CallingConventionEx.StdCall, i);
                    
                    var txt = m.Read<ItemTxt>(pText);
                    w.WriteLine("{0}\t{1}", i, txt.GetCode());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().ToString() + ": " + e.Message);
            }

            w.Close();

            m.ResumeAllThreads();

            //Console.ReadKey();
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ItemTxt
        {
            [FieldOffset(0)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string szFlippyFile;     // 0x00
            [FieldOffset(0x20)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string szInvFile;        // 0x20
            [FieldOffset(0x40)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string szUniqueInvFile;  // 0x40
            [FieldOffset(0x60)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string szSetInvFile;     // 0x60
            [FieldOffset(0x80)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
            public byte[] szCode;           // 0x40
            [FieldOffset(0x10F)]
            public byte xSize;              // 0x10F
            [FieldOffset(0x110)]
            public byte ySize;              // 0x110

            public string GetCode()
            {
                return Encoding.ASCII.GetString(szCode).Replace(" ", "");
            }

            public uint GetDwCode()
            {
                return BitConverter.ToUInt32(szCode, 0) & 0xFFF;
            }
        }

        static void TestBreakPoints()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

            if (!WinApi.SetDebugPrivileges())
            {
                Console.WriteLine("Failed to set debug privileges");
                return;
            }

            try
            {
                var processes = Process.GetProcessesByName("Game");
                //var processes = Process.GetProcessesByName("program");
                if (processes.Length == 0)
                {
                    Console.WriteLine("Could not get process");
                    return;
                }
                else
                    Console.WriteLine("Process found");

                var process = processes.First();
                Console.WriteLine("Process Id: {0} Handle: {1} Threads: {2}", process.Id, process.Handle, process.Threads.Count);
                if (process.Threads.Count == 0)
                {
                    Console.WriteLine("WTF? No threads in process");
                    return;
                }

                Console.WriteLine(FileVersionInfo.GetVersionInfo(process.MainModule.FileVersionInfo.FileName));

                foreach (ProcessModule module in process.Modules)
                    Console.WriteLine("{0} - 0x{1:X}", module.ModuleName, (uint)module.BaseAddress);

                pd = new ProcessDebugger(process.Id);
                Thread th = ProcessDebugger.Run(ref pd);
                if (!pd.WaitForComeUp(500))
                {
                    Console.WriteLine("Failed to start thread");
                    return;
                }

                //var bp = new IncBreakPoint(0x4012B0 - 0x400000, 1, HardwareBreakPoint.Condition.Code);
                //pd.AddBreakPoint("program.exe", bp);


                //Console.WriteLine("Module count: {0}", process.Modules.Count);
                //pd.LoadModule("msscp.dll");

                //Thread.Sleep(1000);
                //process.Refresh();
                //Console.WriteLine("Module count: {0}", process.Modules.Count);

                th.Join();
            }
            catch (BreakPointException e)
            {
                Console.WriteLine("Breakpoint Exception: \"{0}\"", e.Message);
            }
            catch (DebuggerException e)
            {
                Console.WriteLine("Debugger Exception: \"{0}\"", e.Message);
            }
        }
    }
}
