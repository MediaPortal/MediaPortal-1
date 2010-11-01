using System;
using System.Collections.Generic;
using System.Text;

namespace TsPacketChecker
{
  class TsHeader
  {
    public byte SyncByte;
    public bool TransportError;
    public bool PayloadUnitStart;
    public bool TransportPriority;
    public ushort Pid;
    public byte TScrambling;
    public byte AdaptionControl;
    public byte ContinuityCounter;
    public byte AdaptionFieldLength;
    public byte PayLoadStart;
    public bool HasAdaptionField;
    public bool HasPayload;

    public TsHeader()
    {
      TransportError = true;
    }
    public TsHeader(byte[] tsPacket)
    {
      Decode(tsPacket);
    }
    public void Decode(byte[] tsPacket)
    {
      SyncByte = tsPacket[0];
      if (SyncByte != 0x47)
      {
        TransportError = true;
        return;
      }
      TransportError = ((tsPacket[1] & 0x80) ==0x80);
      if (TransportError)
        return;
      PayloadUnitStart = ((tsPacket[1] & 0x40) == 0x40);
      TransportPriority = ((tsPacket[1] & 0x20) == 0x20);
      Pid = (ushort)(((tsPacket[1] & 0x1F) << 8) + tsPacket[2]);
      TScrambling = (byte)(tsPacket[3] & 0xC0);
      AdaptionControl = (byte)((tsPacket[3] >> 4) & 0x3);
      HasAdaptionField = (tsPacket[3] & 0x20) ==0x20;
      HasPayload = (tsPacket[3] & 0x10) == 0x10;
      ContinuityCounter = (byte)(tsPacket[3] & 0x0F);
      AdaptionFieldLength = 0;
      PayLoadStart = 4;
      if (HasAdaptionField)
      {
        AdaptionFieldLength = tsPacket[4];
        PayLoadStart = (byte)(5 + AdaptionFieldLength);
      }
    }
  }
}
