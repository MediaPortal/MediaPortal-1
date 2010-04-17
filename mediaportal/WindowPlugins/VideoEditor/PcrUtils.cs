#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;

namespace TsCutterPackage
{
  public class Pcr
  {
    #region Variables

    public UInt64 ReferenceBase = 0;
    public UInt64 ReferenceExtension = 0;
    public bool isValid = false;

    #endregion

    #region constructors

    public Pcr()
    {
      Reset();
    }

    public Pcr(byte[] tsPacket)
    {
      UInt64 k = tsPacket[6];
      k <<= 25;
      ReferenceBase += k; // bit 25-32 
      k = tsPacket[7];
      k <<= 17;
      ReferenceBase += k; // bit 17-24
      k = tsPacket[8];
      k <<= 9;
      ReferenceBase += k; // bit 9-16
      k = tsPacket[9];
      k <<= 1;
      ReferenceBase += k; // bit 1-8
      k = (ulong)((tsPacket[10] >> 7) & 0x1);
      ReferenceBase += k; // bit 0
      ReferenceExtension = 0;
      k = (ulong)(tsPacket[11] & 0x1);
      k <<= 8;
      ReferenceExtension += k; // bit 8
      k = tsPacket[12];
      ReferenceExtension += k; // bit 0-7
      isValid = true;
    }

    public Pcr(TimeSpan ts)
    {
      double clock = ts.TotalSeconds;
      double khz90Ticks = clock / ((1.0 / 90000.0));
      ReferenceBase = (UInt64)Math.Abs(khz90Ticks);

      clock -= (ReferenceBase * ((1.0 / 90000.0)));
      double mhz27Ticks = clock / ((1.0 / 27000000.0));
      ReferenceExtension = (UInt64)Math.Abs(mhz27Ticks);
      isValid = true;
    }

    #endregion

    #region public functions

    public void Reset()
    {
      ReferenceBase = 0;
      ReferenceExtension = 0;
      isValid = false;
    }

    public DateTime ToDateTime()
    {
      double clock = ((double)(ReferenceBase)) / 90000.0;
      clock += ((double)ReferenceExtension) / 27000000.0;
      DateTime dt = new DateTime(1900, 1, 1);
      return dt.AddSeconds(clock);
    }

    #endregion
  }

  public class PcrUtils
  {
    public static void DecodePtsDts(byte[] tsPacket, ulong offset, out Pcr pts, out Pcr dts)
    {
      pts = new Pcr();
      dts = new Pcr();
      if ((tsPacket[offset + 7] & 0x80) == 0x80)
      {
        pts.isValid = true;
        if ((tsPacket[offset + 7] & 0x40) == 0x40)
        {
          dts.isValid = true;
        }
      }
      if (pts.isValid)
      {
        UInt64 ptsTicks = 0;
        UInt64 k;
        k = (ulong)((tsPacket[offset + 9] >> 1) & 0x7);
        k <<= 30;
        ptsTicks += k; //9: 00101111
        k = tsPacket[offset + 10];
        k <<= 22;
        ptsTicks += k; //10:00110001
        k = (ulong)(tsPacket[offset + 11] >> 1);
        k <<= 15;
        ptsTicks += k; //11:10010001
        k = tsPacket[offset + 12];
        k <<= 7;
        ptsTicks += k; //12:11000011
        k = (ulong)(tsPacket[offset + 13] >> 1);
        ptsTicks += k; //13:11010111
        pts.ReferenceBase = ptsTicks;
      }
      if (dts.isValid)
      {
        UInt64 dtsTicks = 0;
        UInt64 k;
        k = (ulong)((tsPacket[offset + 14] >> 1) & 0x7);
        k <<= 30;
        dtsTicks += k;
        k = tsPacket[offset + 15];
        k <<= 22;
        dtsTicks += k;
        k = (ulong)(tsPacket[offset + 16] >> 1);
        k <<= 15;
        dtsTicks += k;
        k = tsPacket[offset + 17];
        k <<= 7;
        dtsTicks += k;
        k = (ulong)tsPacket[offset + 18] >> 1;
        dtsTicks += k;
        dts.ReferenceBase = dtsTicks;
      }
    }

    #region Patching routines

    public static void PatchPcr(ref byte[] tsPacket, TimeSpan newTimeSpan)
    {
      Pcr pcr = new Pcr(newTimeSpan);
      tsPacket[6] = (byte)(((pcr.ReferenceBase >> 25) & 0xff));
      tsPacket[7] = (byte)(((pcr.ReferenceBase >> 17) & 0xff));
      tsPacket[8] = (byte)(((pcr.ReferenceBase >> 9) & 0xff));
      tsPacket[9] = (byte)(((pcr.ReferenceBase >> 1) & 0xff));
      tsPacket[10] = (byte)(((pcr.ReferenceBase & 0x1) << 7) + 0x7e + ((pcr.ReferenceExtension >> 8) & 0x1));
      tsPacket[11] = (byte)(pcr.ReferenceExtension & 0xff);
    }

    public static void PatchPts(ref byte[] tsPacket, ulong offset, TimeSpan newTimeSpan)
    {
      Pcr pcr = new Pcr(newTimeSpan);
      tsPacket[offset + 13] = (byte)((((pcr.ReferenceBase & 0x7f) << 1) + 1));
      pcr.ReferenceBase >>= 7;
      tsPacket[offset + 12] = (byte)((pcr.ReferenceBase & 0xff));
      pcr.ReferenceBase >>= 8;
      tsPacket[offset + 11] = (byte)((((pcr.ReferenceBase & 0x7f) << 1) + 1));
      pcr.ReferenceBase >>= 7;
      tsPacket[offset + 10] = (byte)((pcr.ReferenceBase & 0xff));
      pcr.ReferenceBase >>= 8;
      tsPacket[offset + 9] = (byte)((((pcr.ReferenceBase & 7) << 1) + 0x31));
    }

    public static void PatchDts(ref byte[] tsPacket, ulong offset, TimeSpan newTimeSpan)
    {
      Pcr pcr = new Pcr(newTimeSpan);
      tsPacket[offset + 18] = (byte)((((pcr.ReferenceBase & 0x7f) << 1) + 1));
      pcr.ReferenceBase >>= 7;
      tsPacket[offset + 17] = (byte)((pcr.ReferenceBase & 0xff));
      pcr.ReferenceBase >>= 8;
      tsPacket[offset + 16] = (byte)((((pcr.ReferenceBase & 0x7f) << 1) + 1));
      pcr.ReferenceBase >>= 7;
      tsPacket[offset + 15] = (byte)((pcr.ReferenceBase & 0xff));
      pcr.ReferenceBase >>= 8;
      tsPacket[offset + 14] = (byte)((((pcr.ReferenceBase & 7) << 1) + 0x11));
    }

    #endregion
  }
}