using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
  Guid("59f8d617-92fd-48d5-8f6d-a97bfd95c448"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsVideoAnalyzer
  {
    [PreserveSig]
    int SetVideoPid(int videoPid);

    [PreserveSig]
    int GetVideoPid(out int videoPid);

    [PreserveSig]
    int SetAudioPid(int audioPid);

    [PreserveSig]
    int GetAudioPid(out int audioPid);

    [PreserveSig]
    int IsVideoEncrypted(out int yesNo);

    [PreserveSig]
    int IsAudioEncrypted(out int yesNo);

    [PreserveSig]
    int Reset();
  }
}
