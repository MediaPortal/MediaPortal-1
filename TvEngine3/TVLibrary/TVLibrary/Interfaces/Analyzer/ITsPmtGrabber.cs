using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
  Guid("6E714740-803D-4175-BEF6-67246BDF1855"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsPmtGrabber
  {
    [PreserveSig]
    int SetPmtPid(int pmtPid);

    [PreserveSig]
    int SetCallBack(IPMTCallback callback);

    [PreserveSig]
    int GetPMTData(IntPtr pmt);
  }
}
