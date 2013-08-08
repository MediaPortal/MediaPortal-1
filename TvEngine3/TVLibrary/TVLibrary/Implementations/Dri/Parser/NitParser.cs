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
using DirectShowLib.BDA;

namespace TvLibrary.Implementations.Dri.Parser
{
  // See ATSC A-56 table 5.1.
  public enum AtscTransmissionMedium
  {
    Cable = 0,
    Satellite,
    Mmds,
    Smatv,
    OverTheAir
  }

  public enum WaveformStandard
  {
    Ntsc = 1,
    Pal625,
    Pal525,
    Secam,
    D2Mac,
    Bmac,
    Cmac,
    Dci,       // DigiCipher I
    VideoCipher,
    RcaDss,
    Orion,
    Leitch
  }

  public enum MatrixMode
  {
    Mono = 0,
    DiscreteStereo,
    MatrixStereo
  }

  public enum TransmissionSystem
  {
    ItutAnnex1 = 1, // ITU ETSI cable (DVB-C???)
    ItutAnnex2,     // ITU North American cable (SCTE???)
    ItuR,           // ITU ETSI satellite
    Atsc,
    DigiCipher      // DC II satellite
  }

  public delegate void NitCarrierDefinitionDelegate(AtscTransmissionMedium transmissionMedium, byte index, int carrierFrequency);
  public delegate void NitModulationModeDelegate(AtscTransmissionMedium transmissionMedium, byte index, TransmissionSystem transmissionSystem,
    BinaryConvolutionCodeRate innerCodingMode, bool isSplitBitstreamMode, ModulationType modulationFormat, int symbolRate);

  /// <summary>
  /// ATSC/SCTE network information table parser. Refer to ATSC A-56 and SCTE 65.
  /// </summary>
  public class NitParser : BaseDriParser
  {
    private enum TableSubtype
    {
      CarrierDefinition = 1,
      ModulationMode,
      SatelliteInformation,
      TransponderData
    }

    private enum FrequencyBand
    {
      Cband = 0,
      KuBandFss,
      KuBandBss
    }

    public event TableCompleteDelegate OnTableComplete = null;
    public event NitCarrierDefinitionDelegate OnCarrierDefinition = null;
    public event NitModulationModeDelegate OnModulationMode = null;

    public NitParser()
      : base(1, 4)
    {
    }

    public void Decode(byte[] section)
    {
      if (OnTableComplete == null)
      {
        return;
      }
      if (section.Length < 13)
      {
        Log.Log.Error("NIT: invalid section size {0}, expected at least 13 bytes", section.Length);
        return;
      }

      byte tableId = section[2];
      if (tableId != 0xc2)
      {
        return;
      }
      int sectionLength = ((section[3] & 0xf) << 8) + section[4];
      if (section.Length != 2 + sectionLength + 3)  // 2 for section length bytes, 3 for table ID and PID
      {
        Log.Log.Error("NIT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
        return;
      }
      int protocolVersion = (section[5] & 0x1f);
      byte firstIndex = section[6];
      byte numberOfRecords = section[7];
      AtscTransmissionMedium transmissionMedium = (AtscTransmissionMedium)(section[8] >> 4);
      TableSubtype tableSubtype = (TableSubtype)(section[8] & 0x0f);
      if ((tableSubtype != TableSubtype.CarrierDefinition || OnCarrierDefinition == null) &&
        (tableSubtype != TableSubtype.ModulationMode || OnModulationMode == null))
      {
        return;
      }

      int pointer = 9;
      int endOfSection = section.Length - 4;

      byte satelliteId = 0;
      if (tableSubtype == TableSubtype.TransponderData)
      {
        if (pointer >= endOfSection)
        {
          Log.Log.Error("NIT: invalid section length at satellite ID, pointer = {0}, end of section = {1}", pointer, endOfSection);
          return;
        }
        satelliteId = section[pointer++];
      }
      Log.Log.Debug("NIT: section length = {0}, protocol version = {1}, first index = {2}, number of records = {3}, transmission medium = {4}, table subtype = {5}, satellite ID = {6}",
        sectionLength, protocolVersion, firstIndex, numberOfRecords, transmissionMedium, tableSubtype, satelliteId);

      for (byte i = 0; i < numberOfRecords; i++)
      {
        try
        {
          switch (tableSubtype)
          {
            case TableSubtype.CarrierDefinition:
              DecodeCarrierDefinition(section, endOfSection, ref pointer, ref firstIndex, transmissionMedium);
              break;
            case TableSubtype.ModulationMode:
              DecodeModulationMode(section, endOfSection, ref pointer, ref firstIndex, transmissionMedium);
              break;
            case TableSubtype.SatelliteInformation:
              DecodeSatelliteInformation(section, endOfSection, ref pointer);
              break;
            case TableSubtype.TransponderData:
              DecodeTransponderData(section, endOfSection, ref pointer);
              break;
            default:
              Log.Log.Error("NIT: unsupported table subtype {0}", tableSubtype);
              return;
          }
        }
        catch (Exception ex)
        {
          Log.Log.Error(ex.Message);
          return;
        }

        // table descriptors
        if (pointer >= endOfSection)
        {
          Log.Log.Error("NIT: invalid section length at table descriptor count, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i);
          return;
        }
        byte descriptorCount = section[pointer++];
        for (byte d = 0; d < descriptorCount; d++)
        {
          if (pointer + 2 > endOfSection)
          {
            Log.Log.Error("NIT: detected table descriptor count {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", descriptorCount, pointer, endOfSection, i, d);
            return;
          }
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NIT: table descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            Log.Log.Error("NIT: invalid table descriptor length {0}, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", length, pointer, endOfSection, i, d);
            return;
          }
          pointer += length;
        }
      }

      while (pointer + 1 < endOfSection)
      {
        byte tag = section[pointer++];
        byte length = section[pointer++];
        Log.Log.Debug("NIT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
        if (pointer + length > endOfSection)
        {
          Log.Log.Error("NIT: invalid descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection);
          return;
        }

        if (tag == 0x93)
        {
          DecodeRevisionDetectionDescriptor(section, pointer, length, (int)tableSubtype);
        }

        pointer += length;
      }

      if (pointer != endOfSection)
      {
        Log.Log.Error("NIT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
        return;
      }

      if (tableSubtype == TableSubtype.CarrierDefinition &&
        (
          _currentVersions[(int)TableSubtype.CarrierDefinition] == -1 ||
          _unseenSections[(int)TableSubtype.CarrierDefinition].Count == 0
        ) &&
        OnCarrierDefinition != null)
      {
        OnCarrierDefinition = null;
        if (OnModulationMode == null && OnTableComplete != null)
        {
          OnTableComplete(MgtTableType.NitCds);
          OnTableComplete = null;
        }
      }
      else if (tableSubtype == TableSubtype.ModulationMode &&
        (
          _currentVersions[(int)TableSubtype.ModulationMode] == -1 ||
          _unseenSections[(int)TableSubtype.ModulationMode].Count == 0
        ) &&
        OnModulationMode != null)
      {
        OnModulationMode = null;
        if (OnCarrierDefinition == null && OnTableComplete != null)
        {
          OnTableComplete(MgtTableType.NitMms);
          OnTableComplete = null;
        }
      }
    }

    private void DecodeCarrierDefinition(byte[] section, int endOfSection, ref int pointer, ref byte firstIndex, AtscTransmissionMedium transmissionMedium)
    {
      if (pointer + 5 > endOfSection)
      {
        throw new Exception(string.Format("NIT: corruption detected at carrier definition, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }
      byte numberOfCarriers = section[pointer++];
      int spacingUnit = 10;   // kHz
      if ((section[pointer] & 0x80) != 0)
      {
        spacingUnit = 125;    // kHz
      }
      int frequencySpacing = spacingUnit * (((section[pointer] & 0x3f) << 8) + section[pointer + 1]);   // kHz
      pointer += 2;

      int frequencyUnit = 10; // kHz
      if ((section[pointer] & 0x80) != 0)
      {
        frequencyUnit = 125;  // kHz
      }
      int firstCarrierFrequency = frequencyUnit * (((section[pointer] & 0x7f) << 8) + section[pointer + 1]);  // kHz
      pointer += 2;
      Log.Log.Debug("NIT: carrier definition, number of carriers = {0}, spacing unit = {1} kHz, frequency spacing = {2} kHz, frequency unit = {3} kHz, first carrier frequency = {4} kHz",
        numberOfCarriers, spacingUnit, frequencySpacing, frequencyUnit, firstCarrierFrequency);

      if (OnCarrierDefinition != null)
      {
        int carrierFrequency = firstCarrierFrequency;
        for (byte f = 0; f < numberOfCarriers; f++)
        {
          OnCarrierDefinition(transmissionMedium, firstIndex++, carrierFrequency);
          carrierFrequency += frequencySpacing;
        }
      }
    }

    private void DecodeModulationMode(byte[] section, int endOfSection, ref int pointer, ref byte firstIndex, AtscTransmissionMedium transmissionMedium)
    {
      if (pointer + 6 > endOfSection)
      {
        throw new Exception(string.Format("NIT: corruption detected at modulation mode, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }
      TransmissionSystem transmissionSystem = (TransmissionSystem)(section[pointer] >> 4);
      BinaryConvolutionCodeRate innerCodingMode = BinaryConvolutionCodeRate.RateNotDefined;
      switch (section[pointer] & 0x0f)
      {
        case 0:
          innerCodingMode = BinaryConvolutionCodeRate.Rate5_11;
          break;
        case 1:
          innerCodingMode = BinaryConvolutionCodeRate.Rate1_2;
          break;
        case 3:
          innerCodingMode = BinaryConvolutionCodeRate.Rate3_5;
          break;
        case 5:
          innerCodingMode = BinaryConvolutionCodeRate.Rate2_3;
          break;
        case 7:
          innerCodingMode = BinaryConvolutionCodeRate.Rate3_4;
          break;
        case 8:
          innerCodingMode = BinaryConvolutionCodeRate.Rate4_5;
          break;
        case 9:
          innerCodingMode = BinaryConvolutionCodeRate.Rate5_6;
          break;
        case 11:
          innerCodingMode = BinaryConvolutionCodeRate.Rate7_8;
          break;
        case 15:
          // concatenated coding not used
          innerCodingMode = BinaryConvolutionCodeRate.RateNotSet;
          break;
      }
      pointer++;

      bool isSplitBitstreamMode = ((section[pointer] & 0x80) != 0);
      ModulationType modulationFormat = ModulationType.ModNotSet;
      switch (section[pointer] & 0x1f)
      {
        case 1:
          modulationFormat = ModulationType.ModQpsk;
          break;
        case 2:
          modulationFormat = ModulationType.ModBpsk;
          break;
        case 3:
          modulationFormat = ModulationType.ModOqpsk;
          break;
        case 4:
          modulationFormat = ModulationType.Mod8Vsb;
          break;
        case 5:
          modulationFormat = ModulationType.Mod16Vsb;
          break;
        case 6:
          modulationFormat = ModulationType.Mod16Qam;
          break;
        case 7:
          modulationFormat = ModulationType.Mod32Qam;
          break;
        case 8:
          modulationFormat = ModulationType.Mod64Qam;
          break;
        case 9:
          modulationFormat = ModulationType.Mod80Qam;
          break;
        case 10:
          modulationFormat = ModulationType.Mod96Qam;
          break;
        case 11:
          modulationFormat = ModulationType.Mod112Qam;
          break;
        case 12:
          modulationFormat = ModulationType.Mod128Qam;
          break;
        case 13:
          modulationFormat = ModulationType.Mod160Qam;
          break;
        case 14:
          modulationFormat = ModulationType.Mod192Qam;
          break;
        case 15:
          modulationFormat = ModulationType.Mod224Qam;
          break;
        case 16:
          modulationFormat = ModulationType.Mod256Qam;
          break;
        case 17:
          modulationFormat = ModulationType.Mod320Qam;
          break;
        case 18:
          modulationFormat = ModulationType.Mod384Qam;
          break;
        case 19:
          modulationFormat = ModulationType.Mod448Qam;
          break;
        case 20:
          modulationFormat = ModulationType.Mod512Qam;
          break;
        case 21:
          modulationFormat = ModulationType.Mod640Qam;
          break;
        case 22:
          modulationFormat = ModulationType.Mod768Qam;
          break;
        case 23:
          modulationFormat = ModulationType.Mod896Qam;
          break;
        case 24:
          modulationFormat = ModulationType.Mod1024Qam;
          break;
      }
      pointer++;

      // s/s
      int symbolRate = ((section[pointer] & 0x0f) << 24) + (section[pointer + 1] << 16) + (section[pointer + 2] << 8) + section[pointer + 3];
      pointer += 4;
      Log.Log.Debug("NIT: modulation mode, transmission system = {0}, inner coding mode = {1}, is split bitstream mode = {2}, modulation format = {3}, symbol rate = {4} s/s",
        transmissionSystem, innerCodingMode, isSplitBitstreamMode, modulationFormat, symbolRate);

      if (OnModulationMode != null)
      {
        OnModulationMode(transmissionMedium, firstIndex++, transmissionSystem, innerCodingMode, isSplitBitstreamMode, modulationFormat, symbolRate);
      }
    }

    private void DecodeSatelliteInformation(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 4 > endOfSection)
      {
        throw new Exception(string.Format("NIT: corruption detected at satellite information, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }
      byte satelliteId = section[pointer++];
      bool youAreHere = ((section[pointer] & 0x80) != 0);
      FrequencyBand frequencyBand = (FrequencyBand)((section[pointer] >> 5) & 0x03);
      bool outOfService = ((section[pointer] & 0x10) != 0);
      bool isEasternHemisphere = ((section[pointer] & 0x08) != 0);
      int orbitalPosition = ((section[pointer] & 0x03) << 8) + section[pointer + 1];
      pointer += 2;
      bool isCircularPolarisation = ((section[pointer] & 0x80) != 0);
      int numberOfTransponders = (section[pointer++] & 0x3f) + 1;
      Log.Log.Debug("NIT: satellite information, satellite ID = {0}, you are here = {1}, frequency band = {2}, out of service = {3}, is Eastern hemisphere = {4}, orbital position = {5}, is circular polarisation = {6}, number of transponders = {7}",
        satelliteId, youAreHere, frequencyBand, outOfService, isEasternHemisphere, orbitalPosition, isCircularPolarisation, numberOfTransponders);
    }

    private void DecodeTransponderData(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 6 > endOfSection)
      {
        throw new Exception(string.Format("NIT: corruption detected at transponder data, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }
      bool isMpeg2Transport = ((section[pointer] & 0x80) == 0);
      bool isVerticalRightPolarisation = ((section[pointer] & 0x40) != 0);
      int transponderNumber = (section[pointer++] & 0x3f);
      byte cdsReference = section[pointer++];
      Log.Log.Debug("NIT: transponder data, is MPEG 2 transport = {0}, is vertical/right polarisation = {1}, transponder number = {2}, CDS reference = {3}",
        isMpeg2Transport, isVerticalRightPolarisation, transponderNumber, cdsReference);
      if (isMpeg2Transport)
      {
        byte mmsReference = section[pointer++];
        int vctId = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        bool isRootTransponder = ((section[pointer++] & 0x80) != 0);
        Log.Log.Debug("NIT: MPEG 2 transponder data, MMS reference = {0}, VCT ID = 0x{1:x}, is root transponder = {2}", mmsReference, vctId, isRootTransponder);
      }
      else
      {
        bool isWideBandwidthVideo = ((section[pointer] & 0x80) != 0);
        WaveformStandard waveformStandard = (WaveformStandard)(section[pointer++] & 0x1f);
        bool isWideBandwidthAudio = ((section[pointer] & 0x80) != 0);
        bool isCompandedAudio = ((section[pointer] & 0x40) != 0);
        MatrixMode matrixMode = (MatrixMode)((section[pointer] >> 4) & 0x03);
        int subcarrier2Offset = 10 * (((section[pointer] & 0x0f) << 6) + (section[pointer + 1] >> 2));  // kHz
        pointer++;
        int subcarrier1Offset = 10 * (((section[pointer] & 0x03) << 8) + section[pointer + 1]);
        pointer += 2;
        Log.Log.Debug("NIT: non-MPEG 2 transponder data, is WB video = {0}, waveform standard = {1}, is WB audio = {2}, is companded audio = {3}, matrix mode = {4}, subcarrier 2 offset = {5} kHz, subcarrier 1 offset = {6} kHz",
          isWideBandwidthVideo, waveformStandard, isWideBandwidthAudio, isCompandedAudio, matrixMode, subcarrier2Offset, subcarrier1Offset);
      }
    }
  }
}
