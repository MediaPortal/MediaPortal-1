using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DShowNET.MPTSWriter
{

  [ComVisible(true), ComImport,
  Guid("3E05D715-0AE2-4d6a-8EE9-51DB5FBAB72B"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IMPTSRecord
  {
    [PreserveSig]
    int SetRecordingFileName([In, MarshalAs(UnmanagedType.LPStr)]			string strFile);
    [PreserveSig]
    int StartRecord(Int64 startTime);
    [PreserveSig]
    int StopRecord(Int64 startTime);
  }

}
