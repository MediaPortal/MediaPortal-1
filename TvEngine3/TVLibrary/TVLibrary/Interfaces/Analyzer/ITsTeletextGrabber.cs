using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
  Guid("540EA3F3-C2E0-4a96-9FC2-071875962911"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITeletextCallBack
  {
    [PreserveSig]
    int OnTeletextReceived(IntPtr data, short packetCount);
  };

  [ComVisible(true), ComImport,
 Guid("9A9E7592-A178-4a63-A210-910FD7FFEC8C"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsTeletextGrabber
  {
    [PreserveSig]
    int Start();

    [PreserveSig]
    int Stop();

    [PreserveSig]
    int SetTeletextPid(short teletextPid);

    [PreserveSig]
    int SetCallBack(ITeletextCallBack callback);

  }
}
