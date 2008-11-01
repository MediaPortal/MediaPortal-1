namespace xPL
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct XplSource
    {
        public string Vendor;
        public string Device;
        public string Instance;
    }
}

