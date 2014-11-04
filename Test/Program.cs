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
                for (var i = 0; i < 3; ++i)
                {
                    addr = m.FindPattern("uwow.exe", new BytePattern("?? 90 ??"), addr + 1);
                    Console.WriteLine("{0:X}", addr);
                }
            }
        }
    }
}
