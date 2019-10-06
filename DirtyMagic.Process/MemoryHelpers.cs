using System.Runtime.InteropServices;

namespace DirtyMagic
{
    public static class MemoryHelpers
    {
        public static T ReinterpretObject<T>(object @object) where T : struct
        {
            var h = GCHandle.Alloc(@object, GCHandleType.Pinned);
            var t = (T)Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof(T));
            h.Free();

            return t;
        }

        public static byte[] ObjectToBytes<T>(T value)
        {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            return bytes;
        }



    }
}
