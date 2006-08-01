using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
 Guid("5CDAC655-D9FB-4c71-8119-DD07FE86A9CE"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsEpgScanner
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

    [PreserveSig]
    int GrabMHW();

    [PreserveSig]
    int IsMHWReady(out bool yesNo);

    [PreserveSig]
    int GetMHWTitleCount(out Int16 count);

    [PreserveSig]
    int GetMHWTitle(Int16 program, ref Int16 id, ref Int16 transportId, ref Int16 networkId, ref Int16 channelId, ref Int16 programId, ref Int16 themeId, ref Int16 PPV, ref byte Summaries, ref Int16 duration, ref uint dateStart, ref uint timeStart, out IntPtr title, out IntPtr programName);

    [PreserveSig]
    int GetMHWChannel(Int16 channelNr, ref Int16 channelId, ref Int16 networkId, ref Int16 transportId, out IntPtr channelName);

    [PreserveSig]
    int GetMHWSummary(Int16 programId, out IntPtr summary);

    [PreserveSig]
    int GetMHWTheme(Int16 themeId, out IntPtr theme);
  }
}
