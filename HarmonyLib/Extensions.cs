using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MIG.Interfaces
{
    internal static class Extensions
    {
        public unsafe static char* ToCharPointer(this string s)
        {
            s += '\0';
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            int cb = Marshal.SystemDefaultCharSize * bytes.Length;
            IntPtr intPtr = Marshal.AllocHGlobal(cb);
            Marshal.Copy(bytes, 0, intPtr, bytes.Length);
            return (char*)((void*)intPtr);
        }

    }
}
