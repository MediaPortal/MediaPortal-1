#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using System.Globalization;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations
{
  /// <summary>
  /// A class for decoding and encoding Philips Pronto format.
  /// </summary>
  /// <remarks>
  /// This code is based on code from IRSS, improved with information from
  /// various internet sources including:
  /// http://www.remotecentral.com/features/irdisp1.htm
  /// http://www.majority.nl/files/pronto.pdf
  /// http://www.majority.nl/files/prontoirformats.pdf
  /// </remarks>
  public static class Pronto
  {
    private enum CodeType
    {
      RawOscillated = 0x0000,
      RawUnmodulated = 0x0100,
      Rc5 = 0x5000,
      Rc5x = 0x5001,
      Rc6Mode0 = 0x6000,
      Rc6Mode6a = 0x6001,

      // Unsupported, not implemented.
      VariableLength = 0x7000,
      IndexToUdb = 0x8000,
      Nec1 = 0x9000,
      Nec2 = 0x900a,
      Nec3 = 0x900b,
      Nec4 = 0x900c,
      Nec5 = 0x900d,
      Nec6 = 0x900e,
      YamahaNec = 0x9001
    }

    #region constants

    // This is the carrier frequency that will be used to decode and encode
    // Pronto data when the actual carrier frequency is not known. The value is
    // somewhat arbitrary - chosen to optimise the match between typical
    // pulse/space durations and the available resolution (16 bits).
    // => Proto data value 1 = 3 micro-seconds
    private const int DECODE_ENCODE_CARRIER_UNKNOWN_HZ = 333333;

    private const int CARRIER_RC5_HZ = 36000;
    private const int CARRIER_RC6_HZ = 36000;

    private const double PRONTO_CLOCK_MULTIPLIER = 0.241246;
    private const int PRONTO_CARRIER_UNKNOWN = 0;

    // unit = micro-seconds (us)
    private const int SIGNAL_FREE_TIME = 10000;
    private const int SIGNAL_FREE_TIME_RC6 = 2666;

    private static readonly int[] HEADER_RC6_MODE_0 = new int[] { 2666, -889, 444, -889, 444, -444, 444, -444, 444, -889, 889 };
    private static readonly int[] HEADER_RC6_MODE_6A = new int[] { 3111, -889, 444, -444, 444, -444, 444, -889, 444, -889, 889 };

    #endregion

    public static bool Decode(string command, out int carrierFrequency, out int[] timingData)
    {
      carrierFrequency = -1;
      timingData = null;

      try
      {
        if (string.IsNullOrWhiteSpace(command))
        {
          return false;
        }

        string[] temp = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        ushort[] prontoData = new ushort[temp.Length];
        for (int i = 0; i < temp.Length; i++)
        {
          prontoData[i] = ushort.Parse(temp[i], NumberStyles.HexNumber);
        }

        switch ((CodeType)prontoData[0])
        {
          case CodeType.RawOscillated:
          case CodeType.RawUnmodulated:
            return DecodeRaw(prontoData, out carrierFrequency, out timingData);

          case CodeType.Rc5:
            return DecodeRc5(prontoData, out carrierFrequency, out timingData);

          case CodeType.Rc5x:
            return DecodeRc5x(prontoData, out carrierFrequency, out timingData);

          case CodeType.Rc6Mode0:
            return DecodeRc6(prontoData, out carrierFrequency, out timingData);

          case CodeType.Rc6Mode6a:
            return DecodeRc6a(prontoData, out carrierFrequency, out timingData);
        }
      }
      catch
      {
      }
      return false;
    }

    private static bool DecodeRaw(ushort[] prontoData, out int carrierFrequency, out int[] timingData)
    {
      carrierFrequency = -1;
      timingData = null;

      if (prontoData.Length < 5)
      {
        return false;
      }

      int burstPairSequenceSizeOnce = 2 * prontoData[2];
      int burstPairSequenceSizeRepeat = 2 * prontoData[3];
      if (burstPairSequenceSizeOnce == 0 && burstPairSequenceSizeRepeat == 0)
      {
        return false;
      }
      int repeatStartIndex = 4 + burstPairSequenceSizeOnce;
      if (burstPairSequenceSizeRepeat == 0)
      {
        burstPairSequenceSizeRepeat = burstPairSequenceSizeOnce;
        repeatStartIndex = 4;
      }

      ushort prontoCarrier = prontoData[1];
      double decodeCarrier = DECODE_ENCODE_CARRIER_UNKNOWN_HZ;
      if (prontoCarrier != PRONTO_CARRIER_UNKNOWN)
      {
        decodeCarrier = CarrierFrequencyFromProntoCarrier(prontoCarrier);
      }
      double multiplier = 1000000 / decodeCarrier;

      int repeatCount = 0;  // can be changed
      if (burstPairSequenceSizeOnce == 0)
      {
        repeatCount++;
      }
      timingData = new int[burstPairSequenceSizeOnce + (repeatCount * burstPairSequenceSizeRepeat)];
      if (timingData.Length == 0)
      {
        return false;
      }

      short pulse = 1;
      int pdIndex = 4;
      int tdIndex = 0;
      while (tdIndex < burstPairSequenceSizeOnce)
      {
        timingData[tdIndex++] = (int)(multiplier * prontoData[pdIndex++] * pulse);  // number of carrier frequency periods => duration in micro-seconds
        pulse *= -1;
      }

      for (int r = 0; r < repeatCount; r++)
      {
        pdIndex = repeatStartIndex;
        for (int i = 0; i < burstPairSequenceSizeRepeat; i++)
        {
          timingData[tdIndex++] = (int)(multiplier * prontoData[pdIndex++] * pulse);
          pulse *= -1;
        }
      }

      carrierFrequency = (int)decodeCarrier;
      if (prontoData[0] == (ushort)CodeType.RawUnmodulated)
      {
        carrierFrequency = 0;   // DC - no carrier
      }
      else if (prontoCarrier == PRONTO_CARRIER_UNKNOWN)
      {
        carrierFrequency = -1;
      }
      return true;
    }

    private static bool DecodeRc5(ushort[] prontoData, out int carrierFrequency, out int[] timingData)
    {
      carrierFrequency = -1;
      timingData = null;

      if (prontoData.Length != 6 || prontoData[2] != 0 || prontoData[3] != 1)
      {
        return false;
      }

      ushort system = prontoData[4];
      ushort command = prontoData[5];
      if (system > 31 || command > 127)
      {
        return false;
      }

      ushort rc5 = 0x2800;  // Start and toggle bits set.
      if (command < 64)
      {
        rc5 |= 0x1000;      // field Bit (inverted command bit 6)
      }
      rc5 |= (ushort)((system << 6) | (command & 0x3f));
      DecodeRc5Variant(rc5, prontoData[1], false, out carrierFrequency, out timingData);
      return true;
    }

    private static bool DecodeRc5x(ushort[] prontoData, out int carrierFrequency, out int[] timingData)
    {
      carrierFrequency = -1;
      timingData = null;

      if (prontoData.Length != 8 || prontoData[2] != 0 || prontoData[3] != 2)
      {
        return false;
      }

      ushort system = prontoData[4];
      ushort command = prontoData[5];
      ushort data = prontoData[6];
      if (system > 31 || command > 127 || data > 63)
      {
        return false;
      }

      uint rc5 = 0xa0000; // Start and toggle bits set.
      if (command < 64)
      {
        rc5 |= 0x40000;   // field Bit (inverted command bit 6)
      }
      rc5 |= (uint)((system << 12) | ((command & 0x3f) << 6) | data);
      DecodeRc5Variant(rc5, prontoData[1], true, out carrierFrequency, out timingData);
      return true;
    }

    private static void DecodeRc5Variant(uint command, ushort prontoCarrierOverride, bool isRc5x, out int carrierFrequency, out int[] timingData)
    {
      uint iStart = 0x2000;
      if (isRc5x)
      {
        iStart = 0x80000;
      }

      List<int> timingDataList = new List<int>();
      int duration = 0;
      for (uint i = iStart; i != 0; i >>= 1)
      {
        if (isRc5x && i == 0x800)
        {
          if (duration > 0)
          {
            timingDataList.Add(duration);
            duration = 0;
          }
          duration -= (889 * 4);
        }

        int d = (command & i) != 0 ? -889 : 889;    // 889 us = 32 x carrier frequency periods; logic 0 = pulse + space; logic 1 = space + pulse
        if (Math.Sign(duration) != Math.Sign(d))
        {
          timingDataList.Add(duration);
          duration = 0;
        }

        timingDataList.Add(duration + d);
        duration = -d;
      }

      if (duration > 0)
      {
        timingDataList.Add(duration);
        timingDataList.Add(-SIGNAL_FREE_TIME);
      }
      else
      {
        timingDataList.Add(duration - SIGNAL_FREE_TIME);
      }

      if (prontoCarrierOverride != 0)
      {
        carrierFrequency = CarrierFrequencyFromProntoCarrier(prontoCarrierOverride);
      }
      else
      {
        carrierFrequency = CARRIER_RC5_HZ;
      }
      timingData = timingDataList.ToArray();
    }

    private static bool DecodeRc6(ushort[] prontoData, out int carrierFrequency, out int[] timingData)
    {
      carrierFrequency = -1;
      timingData = null;

      if (prontoData.Length != 6 || prontoData[2] != 0 || prontoData[3] != 1)
      {
        return false;
      }

      ushort system = prontoData[4];
      ushort command = prontoData[5];
      if (system > 255 || command > 255)
      {
        return false;
      }

      ushort rc6 = (ushort)((system << 8) | command);
      DecodeRc6Variant(rc6, prontoData[1], 0, out carrierFrequency, out timingData);
      return true;
    }

    private static bool DecodeRc6a(ushort[] prontoData, out int carrierFrequency, out int[] timingData)
    {
      carrierFrequency = -1;
      timingData = null;

      if (prontoData.Length != 6 || prontoData[2] != 0 || prontoData[3] != 2)
      {
        return false;
      }

      ushort customer = prontoData[4];
      ushort system = prontoData[5];
      ushort command = prontoData[6];
      if ((customer > 127 && customer < 32768) || system > 255 || command > 255)
      {
        return false;
      }

      uint rc6 = (uint)((customer << 16) | (system << 8) | command);
      DecodeRc6Variant(rc6, prontoData[1], (byte)(customer > 127 ? 16 : 8), out carrierFrequency, out timingData);
      return true;
    }

    private static void DecodeRc6Variant(uint command, ushort prontoCarrierOverride, byte customerBitCount, out int carrierFrequency, out int[] timingData)
    {
      uint iStart = (uint)(1 << (15 + customerBitCount));

      List<int> timingDataList;
      if (customerBitCount == 0)
      {
        timingDataList = new List<int>(HEADER_RC6_MODE_0);
      }
      else
      {
        timingDataList = new List<int>(HEADER_RC6_MODE_6A);
      }

      int duration = timingDataList[timingDataList.Count - 1];
      for (uint i = iStart; i != 0; i >>= 1)
      {
        int d = (command & i) != 0 ? 444 : -444;    // 444 us = 16 x carrier frequency periods; logic 0 = space + pulse; logic 1 = pulse + space
        if (Math.Sign(duration) != Math.Sign(d))
        {
          timingDataList.Add(duration);
          duration = 0;
        }

        timingDataList.Add(duration + d);
        duration = -d;
      }

      if (duration > 0)
      {
        timingDataList.Add(duration);
        timingDataList.Add(-SIGNAL_FREE_TIME_RC6);
      }
      else
      {
        timingDataList.Add(duration - SIGNAL_FREE_TIME_RC6);
      }

      if (prontoCarrierOverride != 0)
      {
        carrierFrequency = CarrierFrequencyFromProntoCarrier(prontoCarrierOverride);
      }
      else
      {
        carrierFrequency = CARRIER_RC6_HZ;
      }
      timingData = timingDataList.ToArray();
    }

    public static string EncodeRaw(int carrierFrequency, ICollection<int> timingData)
    {
      if (timingData == null)
      {
        timingData = new int[0];
      }

      CodeType codeType = CodeType.RawOscillated;
      int encodeCarrier = DECODE_ENCODE_CARRIER_UNKNOWN_HZ;
      ushort prontoCarrier = PRONTO_CARRIER_UNKNOWN;
      if (carrierFrequency == 0)  // DC - no carrier
      {
        codeType = CodeType.RawUnmodulated;
      }
      else if (carrierFrequency > 0)
      {
        encodeCarrier = carrierFrequency;
        prontoCarrier = CarrierFrequencyToProntoCarrier(carrierFrequency);
      }

      string[] prontoData = new string[4 + timingData.Count + (timingData.Count % 2)];
      prontoData[0] = ((ushort)codeType).ToString("X4");
      prontoData[1] = prontoCarrier.ToString("X4");
      prontoData[2] = "0000";                                                 // once burst sequence pair count
      prontoData[3] = ((ushort)((prontoData.Length - 4) / 2)).ToString("X4"); // repeat burst sequence pair count

      double multiplier = carrierFrequency / 1000000;
      int index = 4;
      foreach (int duration in timingData)
      {
        prontoData[index++] = Math.Round(multiplier * Math.Abs(duration)).ToString("X4"); // duration in micro-seconds => number of carrier frequency periods
      }
      if (timingData.Count % 2 != 0)
      {
        prontoData[index] = Math.Round(multiplier * SIGNAL_FREE_TIME).ToString("X4");
      }
      return string.Join(" ", prontoData);
    }

    private static int CarrierFrequencyFromProntoCarrier(ushort prontoCarrier)
    {
      if (prontoCarrier == 0)
      {
        return 0;
      }
      return (int)(1000000 / (prontoCarrier * PRONTO_CLOCK_MULTIPLIER));
    }

    private static ushort CarrierFrequencyToProntoCarrier(int carrierFrequency)
    {
      if (carrierFrequency <= 0)
      {
        return 0;
      }
      return (ushort)(1000000 / (carrierFrequency * PRONTO_CLOCK_MULTIPLIER));
    }
  }
}