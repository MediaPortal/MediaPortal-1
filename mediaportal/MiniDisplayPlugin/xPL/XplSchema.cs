namespace xPL
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct XplSchema
    {
        public string msgClass;
        public string msgType;
    }
}

