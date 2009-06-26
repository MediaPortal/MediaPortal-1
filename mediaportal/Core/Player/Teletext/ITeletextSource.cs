using System;
using System.Runtime.InteropServices;

namespace MediaPortal.Player.Teletext
{
  internal enum TeletextEvent
  {
    SEEK_START = 0,
    SEEK_END = 1,
    RESET = 2,
    BUFFER_IN_UPDATE = 3,
    BUFFER_OUT_UPDATE = 4,
    PACKET_PCR_UPDATE = 5,
    //CURRENT_PCR_UPDATE = 6,
    COMPENSATION_UPDATE = 7
  }

  [Guid("3AB7E208-7962-11DC-9F76-850456D89593"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITeletextSource
  {
    void SetTeletextTSPacketCallback(IntPtr callback);
    void SetTeletextEventCallback(IntPtr callback);
    void SetTeletextServiceInfoCallback(IntPtr callback);
  }
}