using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ECDsaTest.SafeHandles
{
    internal sealed class SafeBignumHandle : SafeHandle
    {
        private SafeBignumHandle() :
            base(IntPtr.Zero, ownsHandle: true)
        {
        }

        internal SafeBignumHandle(IntPtr handle, bool ownsHandle)
            : base(handle, ownsHandle)
        {
        }

        protected override bool ReleaseHandle()
        {
            Interop.Crypto.BigNumDestroy(handle);
            SetHandle(IntPtr.Zero);
            return true;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }
    }
}
