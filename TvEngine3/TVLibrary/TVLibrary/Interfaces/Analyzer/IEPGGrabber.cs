using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{

  [ComVisible(true), ComImport,
  Guid("6301D1B8-6C92-4c6e-8CC2-CD1B05C6B545"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEPGGrabber
  {
    [PreserveSig]
    int GrabEPG();

    [PreserveSig]
    int IsEPGReady(out bool yesNo);

    [PreserveSig]
    int GetEPGChannelCount([Out] out uint channelCount);

    [PreserveSig]
    int GetEPGEventCount([In] uint channel, [Out] out uint eventCount);

    [PreserveSig]
    int GetEPGChannel([In] uint channel, [In, Out] ref UInt16 networkId, [In, Out] ref UInt16 transportid, [In, Out] ref UInt16 service_id);

    [PreserveSig]
    int GetEPGEvent([In] uint channel, [In] uint eventid, [Out] out uint languageCount, [Out] out uint date, [Out] out uint time, [Out] out uint duration, out IntPtr genre);

    [PreserveSig]
    int GetEPGLanguage([In] uint channel, [In] uint eventid, [In]uint languageIndex, [Out] out uint language, [Out] out IntPtr eventText, [Out] out IntPtr eventDescription);
  }


}
