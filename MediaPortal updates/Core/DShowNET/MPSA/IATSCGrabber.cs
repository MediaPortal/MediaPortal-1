using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DShowNET.MPSA
{

  [ComVisible(true), ComImport,
  Guid("3921427B-72AC-4e4d-AF4F-518AFE1D0780"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IATSCGrabber
  {
    [PreserveSig]
    int GrabATSC();

    [PreserveSig]
    int IsATSCReady(out bool yesNo);

    [PreserveSig]
    int GetATSCTitleCount([Out] out UInt16 channelCount);

    [PreserveSig]
    int GetATSCTitle(Int16 no, out Int16 source_id, out uint starttime, out Int16 length_in_mins, out IntPtr title, out IntPtr description);
  }

}
