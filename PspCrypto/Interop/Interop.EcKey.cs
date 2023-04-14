using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static partial class Crypto
    {
        [DllImport(Libraries.CryptoNative, EntryPoint = "CryptoNative_EcKeyDestroy")]
        internal static extern void EcKeyDestroy(IntPtr a);

        [DllImport(Libraries.CryptoNative, EntryPoint = "CryptoNative_EcKeyUpRef")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EcKeyUpRef(IntPtr r);
    }
}
