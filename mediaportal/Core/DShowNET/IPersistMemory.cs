using System;
using System.Collections.Generic;
using System.Text;


using System.Runtime.InteropServices;
namespace DShowNET
{
  [ComVisible(true), ComImport,
  Guid("BD1AE5E0-A6AE-11CE-BD37-504200C10000"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IPersistMemory
  {
    #region "IPersist Methods"
    [PreserveSig]
    int GetClassID(
    [Out]									out Guid pClassID);
    #endregion

    [PreserveSig]
    int IsDirty();

    [PreserveSig]
    int Load([In] IntPtr pMem, [In] uint cbSize);

    [PreserveSig]
    int Save([Out] IntPtr pMem, [In] bool fClearDirty, [In] uint cbSize);

    [PreserveSig]
    int GetSizeMax([Out] out uint pCbSize);

    [PreserveSig]
    int InitNew();
  }
}
