using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  public enum PidType
  {
    Video=0,
    Audio,
    Other
  }

  [ComVisible(true), ComImport, Guid("08177EB2-65D6-4d0a-A2A8-E7B7280A95A3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IVideoAudioObserver
  {
    [PreserveSig] int OnNotify(PidType pidType);
  }

  [ComVisible(true), ComImport, Guid("F94D89B5-C888-4da1-9782-15C1C0CBFE4D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IAnalogVideoAudioObserver
  {
    [PreserveSig]
    int OnNotify(PidType pidType);
  }

}
