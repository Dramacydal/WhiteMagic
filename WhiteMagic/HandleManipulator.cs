using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using WhiteMagic.WinAPI;

namespace WhiteMagic
{
    public static class HandleManipulator
    {
        public static void CloseHandle(IntPtr handle)
        {
            ValidateAsArgument(handle, "handle");

            if (!Kernel32.CloseHandle(handle))
                throw new Win32Exception("Couldn't close the handle correctly.");
        }
    
        public static Process HandleToProcess(SafeMemoryHandle processHandle)
        {
            return Process.GetProcesses().First(p => p.Id == HandleToProcessId(processHandle));
        }

        public static int HandleToProcessId(SafeMemoryHandle processHandle)
        {
            ValidateAsArgument(processHandle, "processHandle");

            var ret = Kernel32.GetProcessId(processHandle);

            if (ret != 0)
                return ret;
            throw new Win32Exception("Couldn't find the process id of the specified handle.");
        }

        public static ProcessThread HandleToThread(SafeMemoryHandle threadHandle)
        {
            foreach (var process in Process.GetProcesses())
            {
                var ret =
                    process.Threads.Cast<ProcessThread>().FirstOrDefault(t => t.Id == HandleToThreadId(threadHandle));
                if (ret != null)
                    return ret;
            }
            throw new InvalidOperationException("Sequence contains no matching element");
        }
      
        public static int HandleToThreadId(SafeMemoryHandle threadHandle)
        {
            ValidateAsArgument(threadHandle, "threadHandle");
            
            var ret = Kernel32.GetThreadId(threadHandle);

            if (ret != 0)
                return ret;

            throw new Win32Exception("Couldn't find the thread id of the specified handle.");
        }
  
        public static void ValidateAsArgument(IntPtr handle, string argumentName)
        {
            if (handle == null)
                throw new ArgumentNullException(argumentName);

            if (handle == IntPtr.Zero)
                throw new ArgumentException("The handle is not valid.", argumentName);
        }
 
        public static void ValidateAsArgument(SafeMemoryHandle handle, string argumentName)
        {
            if (handle == null)
                throw new ArgumentNullException(argumentName);

            if (handle.IsClosed || handle.IsInvalid)
                throw new ArgumentException("The handle is not valid or closed.", argumentName);
        }
    }
}
