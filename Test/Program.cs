using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Magic;
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
                pd.BlackMagic.WriteUInt(addr,
                    pd.BlackMagic.ReadUInt(addr) + 10);

                return true;
            }
        }

        public static ProcessDebugger pd;

        static void Main(string[] args)
        {
            //TestBreakPoints();
            TestMemory();
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

            try
            {
                var printMessage = 0x6FB25EB0;
                var getPlayer = 0x613C0 + 0x6FAB0000;

                //for (var i = 0; i < 50; ++i)
                {
                    var addr = m.AllocateMemory(1024);
                    //Console.WriteLine("{0:X}", addr);
                    m.WriteUTF16String(addr, "asdadsdadssadasd"/* + i.ToString()*/);
                    //var str = m.ReadUTF16String(addr);
                    //Console.WriteLine("{0} {1}", str, Encoding.UTF8.GetByteCount(str));
                    //Console.WriteLine();

                    var asm = new ManagedFasm();
                    asm.Clear();
                    asm.AddLine("push 7");
                    asm.AddLine("push {0}", addr);
                    asm.AddLine("mov eax, {0}", printMessage);
                    asm.AddLine("call eax");
                    asm.AddLine("retn");
                    //asm.AddLine("mov eax, {0}", getPlayer);
                    //asm.AddLine("call eax");
                    //asm.AddLine("retn");

                    var bytes = asm.Assemble();
                    foreach (var b in bytes)
                        Console.Write("{0:X} ", b);
                    Console.WriteLine();

                    //m.WriteBytes(addr + 50, bytes);

                    var exitCode = m.ExecuteRemoteCode(bytes);

                    //Console.WriteLine("Exit code: {0}", exitCode);
                    //var unit = m.Read<UnitAny>(exitCode);
                    //Console.WriteLine(unit.dwAct);


                    m.FreeMemory(addr);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            m.ResumeAllThreads();

            //Console.ReadKey();
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
