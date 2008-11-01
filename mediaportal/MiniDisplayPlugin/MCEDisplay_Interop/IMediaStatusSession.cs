namespace MCEDisplay_Interop
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("A70D81F2-C9D2-4053-AF0E-CDEA39BDD1AD")]
    public interface IMediaStatusSession
    {
        [DispId(1)]
        void MediaStatusChange([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_I4)] MediaStatusPropertyTag[] tags, [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] values);
        [DispId(2)]
        void Close();
    }
}

