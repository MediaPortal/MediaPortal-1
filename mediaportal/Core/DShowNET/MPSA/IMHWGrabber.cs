using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DShowNET.MPSA
{

  [ComVisible(true), ComImport,
  Guid("6F78D59C-1066-4e1b-8258-717F33C51F67"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IMHWGrabber
  {
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
  };
}
