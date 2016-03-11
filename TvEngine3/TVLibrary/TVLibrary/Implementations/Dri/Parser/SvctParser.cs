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

namespace TvLibrary.Implementations.Dri.Parser
{
  public enum ChannelType
  {
    Normal,
    Hidden,
    LocalAccess,
    NvodAccess
  }

  public delegate void SvctChannelDetailDelegate(AtscTransmissionMedium transmissionMedium, int vctId, int virtualChannelNumber, bool applicationVirtualChannel,
    int bitstreamSelect, int pathSelect, ChannelType channelType, int sourceId, byte cdsReference, int programNumber, byte mmsReference);

  public class SvctParser : BaseDriParser
  {
    private enum TableSubtype
    {
      VirtualChannelMap = 0,
      DefinedChannelMap,
      InverseChannelMap
    }

    private enum TransportType
    {
      Mpeg2,
      NonMpeg2
    }

    private enum VideoStandard
    {
      Ntsc = 0,
      Pal625,
      Pal525,
      Secam,
      Mac
    }

    public event TableCompleteDelegate OnTableComplete = null;
    public event SvctChannelDetailDelegate OnChannelDetail = null;

    private HashSet<int> _definedChannels = null;
    private HashSet<int> _channelDefinitions = null;

    public SvctParser()
      : base(0, 2)
    {
      _definedChannels = new HashSet<int>();
      _channelDefinitions = new HashSet<int>();
    }

    public override void Reset()
    {
      base.Reset();
      _definedChannels.Clear();
      _channelDefinitions.Clear();
    }

    public void Decode(byte[] section)
    {
      if (OnTableComplete == null)
      {
        return;
      }
      if (section.Length < 13)
      {
        Log.Log.Error("S-VCT: invalid section size {0}, expected at least 13 bytes", section.Length);
        return;
      }

      byte tableId = section[2];
      if (tableId != 0xc4)
      {
        return;
      }
      int sectionLength = ((section[3] & 0x0f) << 8) + section[4];
      if (section.Length != 2 + sectionLength + 3)  // 2 for section length bytes, 3 for table ID and PID
      {
        Log.Log.Error("S-VCT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
        return;
      }
      byte protocolVersion = (byte)(section[5] & 0x1f);
      AtscTransmissionMedium transmissionMedium = (AtscTransmissionMedium)(section[6] >> 4);
      TableSubtype tableSubtype = (TableSubtype)(section[6] & 0x0f);
      int vctId = (section[7] << 8) + section[8];
      Log.Log.Debug("S-VCT: section length = {0}, protocol version = {1}, transmission medium = {2}, table subtype = {3}, VCT ID = 0x{4:x}",
        sectionLength, protocolVersion, transmissionMedium, tableSubtype, vctId);

      int pointer = 9;
      int endOfSection = section.Length - 4;
      try
      {
        switch (tableSubtype)
        {
          case TableSubtype.DefinedChannelMap:
            DecodeDefinedChannelMap(section, endOfSection, ref pointer);
            break;
          case TableSubtype.VirtualChannelMap:
            DecodeVirtualChannelMap(section, endOfSection, ref pointer, transmissionMedium, vctId);
            break;
          case TableSubtype.InverseChannelMap:
            DecodeInverseChannelMap(section, endOfSection, ref pointer);
            break;
          default:
            Log.Log.Error("S-VCT: unsupported table subtype {0}", tableSubtype);
            return;
        }
      }
      catch (Exception ex)
      {
        Log.Log.Error(ex.Message);
        return;
      }

      while (pointer + 1 < endOfSection)
      {
        byte tag = section[pointer++];
        byte length = section[pointer++];
        Log.Log.Debug("S-VCT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
        if (pointer + length > endOfSection)
        {
          Log.Log.Error("S-VCT: invalid descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection);
          return;
        }

        if (tag == 0x93)  // revision detection descriptor
        {
          DecodeRevisionDetectionDescriptor(section, pointer, length, (int)tableSubtype);
        }

        pointer += length;
      }

      if (pointer != endOfSection)
      {
        Log.Log.Error("S-VCT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
        return;
      }

      // Two methods for detecting S-VCT VCM completion:
      // 1. Revision detection descriptors.
      // 2. DCM channel count equals VCM channel count.
      if (
        (
          tableSubtype == TableSubtype.VirtualChannelMap &&
          _currentVersions[(int)TableSubtype.VirtualChannelMap] != -1 &&
          _unseenSections[(int)TableSubtype.VirtualChannelMap].Count == 0 &&
          OnTableComplete != null
        ) ||
        (
          (tableSubtype == TableSubtype.DefinedChannelMap || tableSubtype == TableSubtype.VirtualChannelMap) &&
          _currentVersions[(int)TableSubtype.VirtualChannelMap] == -1 &&
          _channelDefinitions.Count == _definedChannels.Count &&
          OnTableComplete != null
        )
      )
      {
        OnTableComplete(MgtTableType.SvctVcm);
        OnTableComplete = null;
        OnChannelDetail = null;
      }
    }

    private void DecodeDefinedChannelMap(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 3 > endOfSection)
      {
        throw new Exception(string.Format("S-VCT: corruption detected at defined channel map, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      int firstVirtualChannel = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
      pointer += 2;
      int dcmDataLength = (section[pointer++] & 0x7f);
      if (pointer + dcmDataLength > endOfSection)
      {
        throw new Exception(string.Format("S-VCT: invalid defined channel map data length {0}, pointer = {1}, end of section = {2}", dcmDataLength, pointer, endOfSection));
      }
      Log.Log.Debug("S-VCT: defined channel map, first virtual channel = {0}, DCM data length = {1}", firstVirtualChannel, dcmDataLength);
      int currentChannel = firstVirtualChannel;
      for (int i = 0; i < dcmDataLength; i++)
      {
        bool rangeDefined = ((section[pointer] & 0x80) != 0);
        int channelsCount = (section[pointer++] & 0x7f);
        Log.Log.Debug("S-VCT: range defined = {0}, channels count = {1}", rangeDefined, channelsCount);
        if (rangeDefined)
        {
          for (int c = 0; c < channelsCount; c++)
          {
            _definedChannels.Add(currentChannel++);
          }
        }
        else
        {
          currentChannel += channelsCount;
        }
      }
    }

    private void DecodeVirtualChannelMap(byte[] section, int endOfSection, ref int pointer, AtscTransmissionMedium transmissionMedium, int vctId)
    {
      // Virtual channel formats depend on transmission medium.
      if (pointer + 7 > endOfSection)
      {
        throw new Exception(string.Format("S-VCT: corruption detected at virtual channel map, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      bool freqSpecIncluded = ((section[pointer] & 0x80) != 0);
      bool symbolRateIncluded = ((section[pointer] & 0x40) != 0);
      bool descriptorsIncluded = ((section[pointer++] & 0x20) != 0);
      bool splice = ((section[pointer++] & 0x80) != 0);
      uint activationTime = 0;
      for (byte b = 0; b < 4; b++)
      {
        activationTime = activationTime << 8;
        activationTime = section[pointer++];
      }
      byte numberOfVcRecords = section[pointer++];
      Log.Log.Debug("S-VCT: virtual channel map, transmission medium = {0}, freq. spec. included = {1}, symbol rate included = {2}, descriptors included = {3}, splice = {4}, activation time = {5}, number of VC records = {6}",
        transmissionMedium, freqSpecIncluded, symbolRateIncluded, descriptorsIncluded, splice, activationTime, numberOfVcRecords);

      for (byte i = 0; i < numberOfVcRecords; i++)
      {
        if (pointer + 9 > endOfSection)
        {
          throw new Exception(string.Format("S-VCT: detected number of virtual channel records {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", numberOfVcRecords, pointer, endOfSection, i));
        }

        int virtualChannelNumber = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
        pointer += 2;
        _channelDefinitions.Add(virtualChannelNumber);
        bool applicationVirtualChannel = ((section[pointer] & 0x80) != 0);
        int bitstreamSelect = ((section[pointer] & 0x40) >> 6);   // broadcast reserved
        int pathSelect = ((section[pointer] & 0x20) >> 5);        // satellite, SMATV, broadcast reserved
        TransportType transportType = (TransportType)((section[pointer] & 0x10) >> 4);
        ChannelType channelType = (ChannelType)(section[pointer++] & 0x0f);
        int sourceId = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        Log.Log.Debug("S-VCT: virtual channel number = {0}, application virtual channel = {1}, bitstream select = {2}, path select = {3}, transport type = {4}, channel type = {5}, source ID = 0x{6:x}",
          virtualChannelNumber, applicationVirtualChannel, bitstreamSelect, pathSelect, transportType, channelType, sourceId);

        if (channelType == ChannelType.NvodAccess)
        {
          int nvodChannelBase = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
          pointer += 2;
          if (transmissionMedium == AtscTransmissionMedium.Smatv)
          {
            pointer += 3;
          }
          else if (transmissionMedium != AtscTransmissionMedium.OverTheAir)
          {
            pointer += 2;
          }
          Log.Log.Debug("S-VCT: NVOD channel base = 0x{0:x}", nvodChannelBase);
        }
        else
        {
          switch (transmissionMedium)
          {
            case AtscTransmissionMedium.Satellite:
              if (transportType == TransportType.Mpeg2)
              {
                byte satellite = section[pointer++];
                int transponder = (section[pointer++] & 0x3f);
                int programNumber = (section[pointer] << 8) + section[pointer + 1];
                pointer += 2;
                Log.Log.Debug("S-VCT: satellite = {0}, transponder = {1}, program number = 0x{2:x}", satellite, transponder, programNumber);
              }
              else
              {
                byte satellite = section[pointer++];
                int transponder = (section[pointer++] & 0x3f);
                pointer += 2;
                Log.Log.Debug("S-VCT: satellite = {0}, transponder = {1}", satellite, transponder);
              }
              break;
            case AtscTransmissionMedium.Smatv:
              if (transportType == TransportType.Mpeg2)
              {
                byte cdsReference = section[pointer++];
                int programNumber = (section[pointer] << 8) + section[pointer + 1];
                pointer += 2;
                byte mmsReference = section[pointer++];
                pointer++;
                Log.Log.Debug("S-VCT: CDS reference = {0}, program number = 0x{1:x}, MMS reference = {2}", cdsReference, programNumber, mmsReference);
              }
              else
              {
                byte cdsReference = section[pointer++];
                bool scrambled = ((section[pointer] & 0x80) != 0);
                VideoStandard videoStandard = (VideoStandard)(section[pointer++] & 0x0f);
                bool isWideBandwidthVideo = ((section[pointer] & 0x80) != 0);
                WaveformStandard waveformStandard = (WaveformStandard)(section[pointer++] & 0x1f);
                bool isWideBandwidthAudio = ((section[pointer] & 0x80) != 0);
                bool isCompandedAudio = ((section[pointer] & 0x40) != 0);
                MatrixMode matrixMode = (MatrixMode)((section[pointer] >> 4) & 0x03);
                int subcarrier2Offset = 10 * (((section[pointer] & 0x0f) << 6) + (section[pointer + 1] >> 2));  // kHz
                pointer++;
                int subcarrier1Offset = 10 * (((section[pointer] & 0x03) << 8) + section[pointer + 1]);
                pointer += 2;
                Log.Log.Debug("S-VCT: CDS reference = {0}, scrambled = {1}, video standard = {2}, is WB video = {3}, waveform standard = {4}, is WB audio = {5}, is companded audio = {6}, matrix mode = {7}, subcarrier 2 offset = {8} kHz, subcarrier 1 offset = {9} kHz",
                  cdsReference, scrambled, videoStandard, isWideBandwidthVideo, waveformStandard, isWideBandwidthAudio,
                  isCompandedAudio, matrixMode, subcarrier2Offset, subcarrier1Offset);
              }
              break;
            case AtscTransmissionMedium.OverTheAir:
              if (transportType == TransportType.Mpeg2)
              {
                int programNumber = (section[pointer] << 8) + section[pointer + 1];
                pointer += 2;
                Log.Log.Debug("S-VCT: program number = 0x{0:x}", programNumber);
              }
              else
              {
                bool scrambled = ((section[pointer] & 0x80) != 0);
                VideoStandard videoStandard = (VideoStandard)(section[pointer++] & 0x0f);
                pointer++;
                Log.Log.Debug("S-VCT: scrambled = {0}, video standard = {1}", scrambled, videoStandard);
              }
              break;
            case AtscTransmissionMedium.Cable:
            case AtscTransmissionMedium.Mmds:
              if (transportType == TransportType.Mpeg2)
              {
                byte cdsReference = section[pointer++];
                int programNumber = (section[pointer] << 8) + section[pointer + 1];
                pointer += 2;
                byte mmsReference = section[pointer++];
                Log.Log.Debug("S-VCT: CDS reference = {0}, program number = 0x{1:x}, MMS reference = {2}", cdsReference, programNumber, mmsReference);
                if (OnChannelDetail != null)
                {
                  OnChannelDetail(transmissionMedium, vctId, virtualChannelNumber, applicationVirtualChannel, bitstreamSelect,
                    pathSelect, channelType, sourceId, cdsReference, programNumber, mmsReference);
                }
              }
              else
              {
                byte cdsReference = section[pointer++];
                bool scrambled = ((section[pointer] & 0x80) != 0);
                VideoStandard videoStandard = (VideoStandard)(section[pointer++] & 0x0f);
                pointer += 2;
                Log.Log.Debug("S-VCT: CDS reference = {0}, scrambled = {1}, video standard = {2}", cdsReference, scrambled, videoStandard);
              }
              break;
            default:
              throw new Exception(string.Format("S-VCT: unsupported transmission medium {0}", transmissionMedium));
          }
        }

        if (freqSpecIncluded || transmissionMedium == AtscTransmissionMedium.OverTheAir)
        {
          int frequencyUnit = 10; // kHz
          if ((section[pointer] & 0x80) != 0)
          {
            frequencyUnit = 125;  // kHz
          }
          int carrierFrequency = frequencyUnit * (((section[pointer] & 0x7f) << 8) + section[pointer + 1]);  // kHz
          pointer += 2;
          Log.Log.Debug("S-VCT: frequency, unit = {0} kHz, carrier = {1} kHz", frequencyUnit, carrierFrequency);
        }
        if (symbolRateIncluded && transmissionMedium != AtscTransmissionMedium.OverTheAir)
        {
          // s/s
          int symbolRate = ((section[pointer] & 0x0f) << 24) + (section[pointer + 1] << 16) + (section[pointer + 2] << 8) + section[pointer + 3];
          pointer += 4;
          Log.Log.Debug("S-VCT: symbol rate = {0} s/s", symbolRate);
        }
        if (descriptorsIncluded)
        {
          if (pointer >= endOfSection)
          {
            throw new Exception(string.Format("S-VCT: invalid section length at virtual channel map descriptor count, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i));
          }
          byte descriptorCount = section[pointer++];
          for (byte d = 0; d < descriptorCount; d++)
          {
            if (pointer + 2 > endOfSection)
            {
              throw new Exception(string.Format("S-VCT: detected virtual channel map descriptor count {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", descriptorCount, pointer, endOfSection, i, d));
            }
            byte tag = section[pointer++];
            byte length = section[pointer++];
            Log.Log.Debug("S-VCT: virtual channel map descriptor, tag = 0x{0:x}, length = {1}", tag, length);
            if (pointer + length > endOfSection)
            {
              throw new Exception(string.Format("S-VCT: invalid virtual channel map descriptor length {0}, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", length, pointer, endOfSection, i, d));
            }
            pointer += length;
          }
        }
      }
    }

    private void DecodeInverseChannelMap(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 3 > endOfSection)
      {
        throw new Exception(string.Format("S-VCT: corruption detected at inverse channel map, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      int firstMapIndex = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
      pointer += 2;
      int recordCount = (section[pointer++] & 0x7f);
      if (pointer + (recordCount * 4) > endOfSection)
      {
        throw new Exception(string.Format("S-VCT: invalid inverse channel map record count {0}, pointer = {1}, end of section = {2}", recordCount, pointer, endOfSection));
      }
      Log.Log.Debug("S-VCT: inverse channel map, first map index = {0}, record count = {1}", firstMapIndex, recordCount);
      for (int i = 0; i < recordCount; i++)
      {
        int sourceId = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        int virtualChannelNumber = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
        pointer += 2;
        Log.Log.Debug("S-VCT: source ID = 0x{0:x}, virtual channel number = {1}", sourceId, virtualChannelNumber);
      }
    }
  }
}
