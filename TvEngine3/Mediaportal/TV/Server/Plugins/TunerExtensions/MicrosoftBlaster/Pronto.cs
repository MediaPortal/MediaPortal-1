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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster
{
  /// <summary>
  /// A class for converting IR commands to and from Philips Pronto format.
  /// </summary>
  /// <remarks>
  /// This code is based on code from IRSS, improved with information from
  /// various internet sources including:
  /// http://www.remotecentral.com/features/irdisp1.htm
  /// http://www.majority.nl/files/pronto.pdf
  /// http://www.majority.nl/files/prontoirformats.pdf
  /// </remarks>
  internal static class Pronto
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
    private const int CARRIER_UNKNOWN_HZ = 333333;

    private const int CARRIER_RC5_HZ = 36000;
    private const int CARRIER_RC6_HZ = 36000;

    private const double PRONTO_CLOCK_MULTIPLIER = 0.241246;

    private const int SIGNAL_FREE_TIME = 10000;
    private const int SIGNAL_FREE_TIME_RC6 = 2666;

    private static readonly int[] HEADER_RC6_MODE_0 = new int[] { 2666, -889, 444, -889, 444, -444, 444, -444, 444, -889, 889 };
    private static readonly int[] HEADER_RC6_MODE_6A = new int[] { 3111, -889, 444, -444, 444, -444, 444, -889, 444, -889, 889 };

    #endregion

    /// <summary>
    /// Convert Pronto data into its corresponding IR command representation.
    /// </summary>
    /// <param name="prontoData">The Pronto data to convert.</param>
    /// <returns>an IR command instance if successful, otherwise <c>null</c></returns>
    public static IrCommand ConvertProntoDataToIrCommand(ushort[] prontoData)
    {
      if (prontoData == null || prontoData.Length == 0)
      {
        throw new ArgumentNullException("prontoData");
      }

      switch ((CodeType)prontoData[0])
      {
        case CodeType.RawOscillated:
        case CodeType.RawUnmodulated:
          return ConvertProntoRawToIrCommand(prontoData);

        case CodeType.Rc5:
          return ConvertProntoRc5ToIrCommand(prontoData);

        case CodeType.Rc5x:
          return ConvertProntoRc5xToIrCommand(prontoData);

        case CodeType.Rc6Mode0:
          return ConvertProntoRc6ToIrCommand(prontoData);

        case CodeType.Rc6Mode6a:
          return ConvertProntoRc6aToIrCommand(prontoData);

        default:
          return null;
      }
    }

    private static IrCommand ConvertProntoRawToIrCommand(ushort[] prontoData)
    {
      if (prontoData.Length < 5)
      {
        return null;
      }

      int burstPairSequenceSizeOnce = 2 * prontoData[2];
      int burstPairSequenceSizeRepeat = 2 * prontoData[3];
      if (burstPairSequenceSizeOnce == 0 && burstPairSequenceSizeRepeat == 0)
      {
        return null;
      }
      int repeatStartIndex = 4 + burstPairSequenceSizeOnce;
      if (burstPairSequenceSizeRepeat == 0)
      {
        burstPairSequenceSizeRepeat = burstPairSequenceSizeOnce;
        repeatStartIndex = 4;
      }

      ushort prontoCarrier = prontoData[1];
      double carrierFrequency = CARRIER_UNKNOWN_HZ;
      if (prontoCarrier != 0)
      {
        carrierFrequency = CarrierFrequencyFromProntoCarrier(prontoCarrier);
      }
      double multiplier = 1000000 / carrierFrequency;

      int repeatCount = 0;  // can be changed
      if (burstPairSequenceSizeOnce == 0)
      {
        repeatCount++;
      }
      int[] timingData = new int[burstPairSequenceSizeOnce + (repeatCount * burstPairSequenceSizeRepeat)];
      if (timingData.Length == 0)
      {
        return null;
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

      int irCommandCarrierFrequency = (int)carrierFrequency;
      if (prontoData[0] == (ushort)CodeType.RawUnmodulated)
      {
        irCommandCarrierFrequency = IrCommand.CARRIER_FREQUENCY_DC_MODE;
      }
      else if (prontoCarrier == 0)
      {
        irCommandCarrierFrequency = IrCommand.CARRIER_FREQUENCY_UNKNOWN;
      }
      return new IrCommand(irCommandCarrierFrequency, timingData);
    }

    private static IrCommand ConvertProntoRc5ToIrCommand(ushort[] prontoData)
    {
      if (prontoData.Length != 6 || prontoData[2] != 0 || prontoData[3] != 1)
      {
        return null;
      }

      ushort system = prontoData[4];
      ushort command = prontoData[5];
      if (system > 31 || command > 127)
      {
        return null;
      }

      ushort rc5 = 0x2800;  // Start and toggle bits set.
      if (command < 64)
      {
        rc5 |= 0x1000;      // field Bit (inverted command bit 6)
      }
      rc5 |= (ushort)((system << 6) | (command & 0x3f));
      return GetIrCommandForRc5Command(rc5, prontoData[1], false);
    }

    private static IrCommand ConvertProntoRc5xToIrCommand(ushort[] prontoData)
    {
      if (prontoData.Length != 8 || prontoData[2] != 0 || prontoData[3] != 2)
      {
        return null;
      }

      ushort system = prontoData[4];
      ushort command = prontoData[5];
      ushort data = prontoData[6];
      if (system > 31 || command > 127 || data > 63)
      {
        return null;
      }

      uint rc5 = 0xa0000; // Start and toggle bits set.
      if (command < 64)
      {
        rc5 |= 0x40000;   // field Bit (inverted command bit 6)
      }
      rc5 |= (uint)((system << 12) | ((command & 0x3f) << 6) | data);
      return GetIrCommandForRc5Command(rc5, prontoData[1], true);
    }

    private static IrCommand GetIrCommandForRc5Command(uint command, ushort prontoCarrierOverride = 0, bool isRc5x = false)
    {
      uint iStart = 0x2000;
      if (isRc5x)
      {
        iStart = 0x80000;
      }

      List<int> timingData = new List<int>();
      int duration = 0;
      for (uint i = iStart; i != 0; i >>= 1)
      {
        if (isRc5x && i == 0x800)
        {
          if (duration > 0)
          {
            timingData.Add(duration);
            duration = 0;
          }
          duration -= (889 * 4);
        }

        int d = (command & i) != 0 ? -889 : 889;    // 889 us = 32 x carrier frequency periods; logic 0 = pulse + space; logic 1 = space + pulse
        if (Math.Sign(duration) != Math.Sign(d))
        {
          timingData.Add(duration);
          duration = 0;
        }

        timingData.Add(duration + d);
        duration = -d;
      }

      if (duration > 0)
      {
        timingData.Add(duration);
        timingData.Add(-SIGNAL_FREE_TIME);
      }
      else
      {
        timingData.Add(duration - SIGNAL_FREE_TIME);
      }

      if (prontoCarrierOverride != 0)
      {
        return new IrCommand(CarrierFrequencyFromProntoCarrier(prontoCarrierOverride), timingData.ToArray());
      }
      return new IrCommand(CARRIER_RC5_HZ, timingData.ToArray());
    }

    private static IrCommand ConvertProntoRc6ToIrCommand(ushort[] prontoData)
    {
      if (prontoData.Length != 6 || prontoData[2] != 0 || prontoData[3] != 1)
      {
        return null;
      }

      ushort system = prontoData[4];
      ushort command = prontoData[5];
      if (system > 255 || command > 255)
      {
        return null;
      }

      ushort rc6 = (ushort)((system << 8) | command);
      return GetIrCommandForRc6Command(rc6, prontoData[1]);
    }

    private static IrCommand ConvertProntoRc6aToIrCommand(ushort[] prontoData)
    {
      if (prontoData.Length != 6 || prontoData[2] != 0 || prontoData[3] != 2)
      {
        return null;
      }

      ushort customer = prontoData[4];
      ushort system = prontoData[5];
      ushort command = prontoData[6];
      if ((customer > 127 && customer < 32768) || system > 255 || command > 255)
      {
        return null;
      }

      uint rc6 = (uint)((customer << 16) | (system << 8) | command);
      return GetIrCommandForRc6Command(rc6, prontoData[1], (byte)(customer > 127 ? 16 : 8));
    }

    private static IrCommand GetIrCommandForRc6Command(uint command, ushort prontoCarrierOverride = 0, byte customerBitCount = 0)
    {
      uint iStart = (uint)(1 << (15 + customerBitCount));

      List<int> timingData;
      if (customerBitCount == 0)
      {
        timingData = new List<int>(HEADER_RC6_MODE_0);
      }
      else
      {
        timingData = new List<int>(HEADER_RC6_MODE_6A);
      }

      int duration = timingData[timingData.Count - 1];
      for (uint i = iStart; i != 0; i >>= 1)
      {
        int d = (command & i) != 0 ? 444 : -444;    // 444 us = 16 x carrier frequency periods; logic 0 = space + pulse; logic 1 = pulse + space
        if (Math.Sign(duration) != Math.Sign(d))
        {
          timingData.Add(duration);
          duration = 0;
        }

        timingData.Add(duration + d);
        duration = -d;
      }

      if (duration > 0)
      {
        timingData.Add(duration);
        timingData.Add(-SIGNAL_FREE_TIME_RC6);
      }
      else
      {
        timingData.Add(duration - SIGNAL_FREE_TIME_RC6);
      }

      if (prontoCarrierOverride != 0)
      {
        return new IrCommand(CarrierFrequencyFromProntoCarrier(prontoCarrierOverride), timingData.ToArray());
      }
      return new IrCommand(CARRIER_RC6_HZ, timingData.ToArray());
    }

    /// <summary>
    /// Convert an IR command instance into its corresponding Pronto [raw]
    /// representation.
    /// </summary>
    /// <param name="command">The IR command to convert.</param>
    /// <returns>Pronto data</returns>
    public static ushort[] ConvertIrCommandToProntoRaw(IrCommand command)
    {
      CodeType codeType = CodeType.RawOscillated;
      int carrierFrequency = CARRIER_UNKNOWN_HZ;
      ushort prontoCarrier = 0;
      if (command.CarrierFrequency == IrCommand.CARRIER_FREQUENCY_DC_MODE)
      {
        codeType = CodeType.RawUnmodulated;
      }
      else if (command.CarrierFrequency != IrCommand.CARRIER_FREQUENCY_UNKNOWN)
      {
        carrierFrequency = command.CarrierFrequency;
        prontoCarrier = CarrierFrequencyToProntoCarrier(carrierFrequency);
      }

      ushort[] prontoData = new ushort[4 + command.TimingData.Length + (command.TimingData.Length % 2)];
      prontoData[0] = (ushort)codeType;
      prontoData[1] = prontoCarrier;
      prontoData[2] = (ushort)((prontoData.Length - 4) / 2);  // once burst sequence pair count
      prontoData[3] = 0;                                      // repeat burst sequence pair count

      double multiplier = carrierFrequency / 1000000;
      int index = 4;
      foreach (int duration in command.TimingData)
      {
        prontoData[index++] = (ushort)Math.Round(multiplier * Math.Abs(duration));  // duration in micro-seconds => number of carrier frequency periods
      }
      if (command.TimingData.Length % 2 != 0)
      {
        prontoData[index] = SIGNAL_FREE_TIME;
      }
      return prontoData;
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