using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
  Guid("1663DC42-D169-41da-BCE2-EEEC482CB9FB"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsChannelScan
  {
    [PreserveSig]
    int Start();

    [PreserveSig]
    int Stop();

    [PreserveSig]
    int GetCount(out short channelCount);

    [PreserveSig]
    int GetChannel(short index,
                       out short networkId,
                       out short transportId,
                       out short serviceId,
                       out short majorChannel,
                       out short minorChannel,
                       out short frequency,
                       out short lcn,
                       out short EIT_schedule_flag,
                       out short EIT_present_following_flag,
                       out short runningStatus,
                       out short freeCAMode,
                       out short serviceType,
                       out short modulation,
                       out IntPtr providerName,
                       out IntPtr serviceName,
                       out short pcrPid,
                       out short pmtPid,
                       out short videoPid,
                       out short audio1Pid,
                       out short audio2Pid,
                       out short audio3Pid,
                       out short ac3Pid,
                       out IntPtr audioLanguage1,
                       out IntPtr audioLanguage2,
                       out IntPtr audioLanguage3,
                       out short teletextPid,
                       out short subtitlePid);
  }
}
