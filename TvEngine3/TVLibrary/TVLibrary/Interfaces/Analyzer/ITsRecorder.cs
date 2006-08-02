using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
 Guid("B45662E3-2749-4a34-993A-0C1659E86E83"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsRecorder
  {
    [PreserveSig]
    int SetPcrPid(short pcrPid);
    [PreserveSig]
    int AddPesStream(short pid);
    [PreserveSig]
    int RemovePesStream(short pid);
    [PreserveSig]
    int SetRecordingFileName([In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    [PreserveSig]
    int StartRecord();
    [PreserveSig]
    int StopRecord();
  }
}
