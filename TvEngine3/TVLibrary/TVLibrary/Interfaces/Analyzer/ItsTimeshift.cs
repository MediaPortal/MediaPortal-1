using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
Guid("89459BF6-D00E-4d28-928E-9DA8F76B6D3A"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsTimeShift
  {
    [PreserveSig]
    int SetPcrPid(short pcrPid);
    [PreserveSig]
    int AddPesStream(short pid);
    [PreserveSig]
    int SetTimeShiftingFileName([In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    [PreserveSig]
    int Start();
    [PreserveSig]
    int Stop();
  }
}
