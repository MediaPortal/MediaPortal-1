using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
  Guid("d5ff805e-a98b-4d56-bede-3f1b8ef72533"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IMPRecord
  {
    [PreserveSig]
    int SetRecordingFileName([In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    [PreserveSig]
    int StartRecord();
    [PreserveSig]
    int StopRecord();
    [PreserveSig]
    int IsReceiving(out bool yesNo);
    [PreserveSig]
    int Reset();
    [PreserveSig]
    int SetTimeShiftFileName([In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    [PreserveSig]
    int StartTimeShifting();
    [PreserveSig]
    int StopTimeShifting();
  }
}
