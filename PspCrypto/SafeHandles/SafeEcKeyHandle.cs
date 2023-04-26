using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ECDsaTest.SafeHandles
{
    internal sealed class SafeEcKeyHandle : SafeHandle
    {
        private SafeEcKeyHandle() :
            base(IntPtr.Zero, ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            Interop.Crypto.EcKeyDestroy(handle);
            SetHandle(IntPtr.Zero);
            return true;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static SafeEcKeyHandle DuplicateHandle(IntPtr handle)
        {
            Debug.Assert(handle != IntPtr.Zero);

            // Reliability: Allocate the SafeHandle before calling EC_KEY_up_ref so
            // that we don't lose a tracked reference in low-memory situations.
            SafeEcKeyHandle safeHandle = new SafeEcKeyHandle();

            if (!Interop.Crypto.EcKeyUpRef(handle))
            {
                throw Interop.Crypto.CreateOpenSslCryptographicException();
            }

            safeHandle.SetHandle(handle);
            return safeHandle;
        }
    }
}
