using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DShowNET.MPTSWriter
{

  [ComVisible(true), ComImport,
  Guid("236D0A77-D105-43fd-A203-578859AB7948"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IMPTSWriter
  {
    [PreserveSig]
    int ResetPids();
    [PreserveSig]
    int SetVideoPid(ushort videoPid);
    [PreserveSig]
    int SetAudioPid(ushort audioPid);
    [PreserveSig]
    int SetAudioPid2(ushort audioPid);
    [PreserveSig]
    int SetAC3Pid(ushort ac3Pid);
    [PreserveSig]
    int SetTeletextPid(ushort ttxtPid);
    [PreserveSig]
    int SetSubtitlePid(ushort subtitlePid);
    [PreserveSig]
    int SetPMTPid(ushort pmtPid);
    [PreserveSig]
    int SetPCRPid(ushort pcrPid);
    [PreserveSig]
    int TimeShiftBufferDuration(out long timeInTimeShiftBuffer);
    [PreserveSig]
    int IsStarted(out ushort yesNo);
  }

}
