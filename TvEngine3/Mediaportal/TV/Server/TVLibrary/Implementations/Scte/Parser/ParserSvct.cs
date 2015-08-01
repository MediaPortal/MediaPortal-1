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
using Mediaportal.TV.Server.TVLibrary.Implementations.Scte.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Scte.Parser
{
  internal delegate void SvctChannelDetailDelegate(TransmissionMedium transmissionMedium, ushort vctId, ushort virtualChannelNumber, bool applicationVirtualChannel,
    byte bitstreamSelect, byte pathSelect, ChannelType channelType, ushort sourceId, byte cdsReference, ushort programNumber, byte mmsReference);

  internal class ParserSvct : ParserBase
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

    private HashSet<int> _definedChannels = null;     // channel numbers
    private HashSet<int> _channelDefinitions = null;  // channel numbers
    private HashSet<int> _hiddenChannels = null;      // channel numbers
    private TableCompleteDelegate _tableCompleteEventDelegate = null;
    private SvctChannelDetailDelegate _channelDetailEventDelegate = null;

    public event TableCompleteDelegate OnTableComplete
    {
      add
      {
        lock (_lock)
        {
          _tableCompleteEventDelegate += value;
        }
      }
      remove
      {
        lock (_lock)
        {
          _tableCompleteEventDelegate -= value;
        }
      }
    }

    public event SvctChannelDetailDelegate OnChannelDetail
    {
      add
      {
        lock (_lock)
        {
          _channelDetailEventDelegate += value;
        }
      }
      remove
      {
        lock (_lock)
        {
          _channelDetailEventDelegate -= value;
        }
      }
    }

    public ParserSvct()
      : base((int)TableSubtype.VirtualChannelMap, (int)TableSubtype.InverseChannelMap)
    {
      _definedChannels = new HashSet<int>();
      _channelDefinitions = new HashSet<int>();
      _hiddenChannels = new HashSet<int>();
    }

    public HashSet<int> DefinedChannels
    {
      get
      {
        return _definedChannels;
      }
    }

    public override void Reset()
    {
      lock (_lock)
      {
        base.Reset();
        _definedChannels.Clear();
        _channelDefinitions.Clear();
        _hiddenChannels.Clear();
      }
    }

    public void Decode(byte[] section)
    {
      lock (_lock)
      {
        if (_tableCompleteEventDelegate == null)
        {
          return;
        }
        if (section.Length < 13)
        {
          this.LogError("S-VCT: invalid section size {0}, expected at least 13 bytes", section.Length);
          return;
        }

        byte tableId = section[2];
        if (tableId != 0xc4)
        {
          return;
        }
        int sectionLength = ((section[3] & 0x0f) << 8) | section[4];
        if (section.Length != 2 + sectionLength + 3)  // 2 for section length bytes, 3 for table ID and PID
        {
          this.LogError("S-VCT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
          return;
        }
        byte protocolVersion = (byte)(section[5] & 0x1f);
        TransmissionMedium transmissionMedium = (TransmissionMedium)(section[6] >> 4);
        TableSubtype tableSubtype = (TableSubtype)(section[6] & 0x0f);
        ushort vctId = (ushort)((section[7] << 8) | section[8]);
        this.LogDebug("S-VCT: section length = {0}, protocol version = {1}, transmission medium = {2}, table subtype = {3}, VCT ID = {4}",
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
              this.LogError("S-VCT: unsupported table subtype {0}", tableSubtype);
              return;
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex.Message);
          return;
        }

        while (pointer + 1 < endOfSection)
        {
          byte tag = section[pointer++];
          byte length = section[pointer++];
          this.LogDebug("S-VCT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            this.LogError("S-VCT: invalid descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection);
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
          this.LogError("S-VCT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
          return;
        }

        // Two methods for detecting S-VCT completion to handle profiles 1 and
        // 2. See SCTE 65 annex A.
        if (
          _tableCompleteEventDelegate != null &&
          (
            // VCM and DCM complete by revision descriptors. Note both VCM and
            // DCM are mandatory.
            _currentVersions[(int)TableSubtype.VirtualChannelMap] != VERSION_NOT_DEFINED &&
            _unseenSections[(int)TableSubtype.VirtualChannelMap].Count == 0 &&
            _currentVersions[(int)TableSubtype.DefinedChannelMap] != VERSION_NOT_DEFINED &&
            _unseenSections[(int)TableSubtype.DefinedChannelMap].Count == 0
          ) ||
          (
            // VCMs received for all defined channels.
            // The meaning of "defined" doesn't seem to be specified by the
            // standard. We support two meanings:
            // 1. DCM set matches VCM set.
            // 2. VCM set is a superset of the DCM and hidden VCM sets.
            _definedChannels.Count > 0 &&
            _currentVersions[(int)TableSubtype.VirtualChannelMap] == VERSION_NOT_DEFINED &&
            _currentVersions[(int)TableSubtype.DefinedChannelMap] == VERSION_NOT_DEFINED &&
            (
              _definedChannels.Count == _channelDefinitions.Count ||
              _definedChannels.Count == _channelDefinitions.Count - _hiddenChannels.Count
            )
          )
        )
        {
          HashSet<int> channelDefinitions = new HashSet<int>(_channelDefinitions);
          channelDefinitions.ExceptWith(_definedChannels);
          channelDefinitions.ExceptWith(_hiddenChannels);
          if (channelDefinitions.Count == 0)
          {
            _tableCompleteEventDelegate(MgtTableType.SvctVcm);
            _tableCompleteEventDelegate = null;
            _channelDetailEventDelegate = null;
          }
        }
      }
    }

    private void DecodeDefinedChannelMap(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 3 > endOfSection)
      {
        throw new TvException("S-VCT: corruption detected at defined channel map, pointer = {0}, end of section = {1}", pointer, endOfSection);
      }

      ushort firstVirtualChannel = (ushort)(((section[pointer] & 0x0f) << 8) | section[pointer + 1]);
      pointer += 2;
      byte dcmDataLength = (byte)(section[pointer++] & 0x7f);
      if (pointer + dcmDataLength > endOfSection)
      {
        throw new TvException("S-VCT: invalid defined channel map data length {0}, pointer = {1}, end of section = {2}", dcmDataLength, pointer, endOfSection);
      }
      this.LogDebug("S-VCT: defined channel map, first virtual channel = {0}, DCM data length = {1}", firstVirtualChannel, dcmDataLength);
      ushort currentChannel = firstVirtualChannel;
      for (byte i = 0; i < dcmDataLength; i++)
      {
        bool rangeDefined = (section[pointer] & 0x80) != 0;
        byte channelsCount = (byte)(section[pointer++] & 0x7f);
        this.LogDebug("S-VCT: range defined = {0}, channels count = {1}", rangeDefined, channelsCount);
        if (rangeDefined)
        {
          for (byte c = 0; c < channelsCount; c++)
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

    private void DecodeVirtualChannelMap(byte[] section, int endOfSection, ref int pointer, TransmissionMedium transmissionMedium, ushort vctId)
    {
      // Virtual channel formats depend on transmission medium.
      if (pointer + 7 > endOfSection)
      {
        throw new TvException("S-VCT: corruption detected at virtual channel map, pointer = {0}, end of section = {1}", pointer, endOfSection);
      }

      bool freqSpecIncluded = (section[pointer] & 0x80) != 0;
      bool symbolRateIncluded = (section[pointer] & 0x40) != 0;
      bool descriptorsIncluded = (section[pointer++] & 0x20) != 0;
      bool splice = (section[pointer++] & 0x80) != 0;
      uint activationTime = 0;
      for (byte b = 0; b < 4; b++)
      {
        activationTime = activationTime << 8;
        activationTime |= section[pointer++];
      }
      byte numberOfVcRecords = section[pointer++];
      this.LogDebug("S-VCT: virtual channel map, transmission medium = {0}, freq. spec. included = {1}, symbol rate included = {2}, descriptors included = {3}, splice = {4}, activation time = {5}, number of VC records = {6}",
        transmissionMedium, freqSpecIncluded, symbolRateIncluded, descriptorsIncluded, splice, activationTime, numberOfVcRecords);

      for (byte i = 0; i < numberOfVcRecords; i++)
      {
        if (pointer + 9 > endOfSection)
        {
          throw new TvException("S-VCT: detected number of virtual channel records {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", numberOfVcRecords, pointer, endOfSection, i);
        }

        ushort virtualChannelNumber = (ushort)(((section[pointer] & 0x0f) << 8) | section[pointer + 1]);
        pointer += 2;
        bool applicationVirtualChannel = (section[pointer] & 0x80) != 0;
        byte bitstreamSelect = (byte)((section[pointer] & 0x40) >> 6);  // broadcast reserved
        byte pathSelect = (byte)((section[pointer] & 0x20) >> 5);       // satellite, SMATV, broadcast reserved
        TransportType transportType = (TransportType)((section[pointer] & 0x10) >> 4);
        ChannelType channelType = (ChannelType)(section[pointer++] & 0x0f);
        ushort sourceId = (ushort)((section[pointer] << 8) | section[pointer + 1]);
        pointer += 2;
        this.LogDebug("S-VCT: virtual channel number = {0}, application virtual channel = {1}, bitstream select = {2}, path select = {3}, transport type = {4}, channel type = {5}, source ID = {6}",
          virtualChannelNumber, applicationVirtualChannel, bitstreamSelect, pathSelect, transportType, channelType, sourceId);

        _channelDefinitions.Add(virtualChannelNumber);
        if (channelType == ChannelType.Hidden)
        {
          _hiddenChannels.Add(virtualChannelNumber);
        }

        if (channelType == ChannelType.NvodAccess)
        {
          ushort nvodChannelBase = (ushort)(((section[pointer] & 0x0f) << 8) | section[pointer + 1]);
          pointer += 2;
          if (transmissionMedium == TransmissionMedium.Smatv)
          {
            pointer += 3;
          }
          else if (transmissionMedium != TransmissionMedium.OverTheAir)
          {
            pointer += 2;
          }
          this.LogDebug("S-VCT: NVOD channel base = {0}", nvodChannelBase);
        }
        else
        {
          switch (transmissionMedium)
          {
            case TransmissionMedium.Satellite:
              if (transportType == TransportType.Mpeg2)
              {
                byte satellite = section[pointer++];
                byte transponder = (byte)(section[pointer++] & 0x3f);
                ushort programNumber = (ushort)((section[pointer] << 8) | section[pointer + 1]);
                pointer += 2;
                this.LogDebug("S-VCT: satellite = {0}, transponder = {1}, program number = {2}", satellite, transponder, programNumber);
              }
              else
              {
                byte satellite = section[pointer++];
                byte transponder = (byte)(section[pointer++] & 0x3f);
                pointer += 2;
                this.LogDebug("S-VCT: satellite = {0}, transponder = {1}", satellite, transponder);
              }
              break;
            case TransmissionMedium.Smatv:
              if (transportType == TransportType.Mpeg2)
              {
                byte cdsReference = section[pointer++];
                ushort programNumber = (ushort)((section[pointer] << 8) | section[pointer + 1]);
                pointer += 2;
                byte mmsReference = section[pointer++];
                pointer++;
                this.LogDebug("S-VCT: CDS reference = {0}, program number = {1}, MMS reference = {2}", cdsReference, programNumber, mmsReference);
              }
              else
              {
                byte cdsReference = section[pointer++];
                bool scrambled = (section[pointer] & 0x80) != 0;
                VideoStandard videoStandard = (VideoStandard)(section[pointer++] & 0x0f);
                bool isWideBandwidthAudio = (section[pointer] & 0x80) != 0;
                bool isCompandedAudio = (section[pointer] & 0x40) != 0;
                MatrixMode matrixMode = (MatrixMode)((section[pointer] >> 4) & 0x03);
                int subcarrier2Offset = 10 * (((section[pointer] & 0x0f) << 6) | (section[pointer + 1] >> 2));  // kHz
                pointer++;
                int subcarrier1Offset = 10 * (((section[pointer] & 0x03) << 8) | section[pointer + 1]);
                pointer += 2;
                this.LogDebug("S-VCT: CDS reference = {0}, scrambled = {1}, video standard = {2}, is WB audio = {3}, is companded audio = {4}, matrix mode = {5}, subcarrier 2 offset = {6} kHz, subcarrier 1 offset = {7} kHz",
                  cdsReference, scrambled, videoStandard, isWideBandwidthAudio,
                  isCompandedAudio, matrixMode, subcarrier2Offset, subcarrier1Offset);
              }
              break;
            case TransmissionMedium.OverTheAir:
              if (transportType == TransportType.Mpeg2)
              {
                ushort programNumber = (ushort)((section[pointer] << 8) | section[pointer + 1]);
                pointer += 2;
                this.LogDebug("S-VCT: program number = {0}", programNumber);
              }
              else
              {
                bool scrambled = (section[pointer] & 0x80) != 0;
                VideoStandard videoStandard = (VideoStandard)(section[pointer++] & 0x0f);
                pointer++;
                this.LogDebug("S-VCT: scrambled = {0}, video standard = {1}", scrambled, videoStandard);
              }
              break;
            case TransmissionMedium.Cable:
            case TransmissionMedium.Mmds:
              if (transportType == TransportType.Mpeg2)
              {
                byte cdsReference = section[pointer++];
                ushort programNumber = (ushort)((section[pointer] << 8) | section[pointer + 1]);
                pointer += 2;
                byte mmsReference = section[pointer++];
                this.LogDebug("S-VCT: CDS reference = {0}, program number = {1}, MMS reference = {2}", cdsReference, programNumber, mmsReference);
                if (_channelDetailEventDelegate != null)
                {
                  _channelDetailEventDelegate(transmissionMedium, vctId, virtualChannelNumber, applicationVirtualChannel, bitstreamSelect,
                    pathSelect, channelType, sourceId, cdsReference, programNumber, mmsReference);
                }
              }
              else
              {
                byte cdsReference = section[pointer++];
                bool scrambled = (section[pointer] & 0x80) != 0;
                VideoStandard videoStandard = (VideoStandard)(section[pointer++] & 0x0f);
                pointer += 2;
                this.LogDebug("S-VCT: CDS reference = {0}, scrambled = {1}, video standard = {2}", cdsReference, scrambled, videoStandard);
              }
              break;
            default:
              throw new TvException("S-VCT: unsupported transmission medium {0}", transmissionMedium);
          }
        }

        if (freqSpecIncluded || transmissionMedium == TransmissionMedium.OverTheAir)
        {
          int frequencyUnit = 10; // kHz
          if ((section[pointer] & 0x80) != 0)
          {
            frequencyUnit = 125;  // kHz
          }
          int carrierFrequency = frequencyUnit * (((section[pointer] & 0x7f) << 8) | section[pointer + 1]);  // kHz
          pointer += 2;
          this.LogDebug("S-VCT: frequency, unit = {0} kHz, carrier = {1} kHz", frequencyUnit, carrierFrequency);
        }
        if (symbolRateIncluded && transmissionMedium != TransmissionMedium.OverTheAir)
        {
          // s/s
          int symbolRate = ((section[pointer] & 0x0f) << 24) | (section[pointer + 1] << 16) | (section[pointer + 2] << 8) | section[pointer + 3];
          pointer += 4;
          this.LogDebug("S-VCT: symbol rate = {0} s/s", symbolRate);
        }
        if (descriptorsIncluded)
        {
          if (pointer >= endOfSection)
          {
            throw new TvException("S-VCT: invalid section length at virtual channel map descriptor count, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i);
          }
          byte descriptorCount = section[pointer++];
          for (byte d = 0; d < descriptorCount; d++)
          {
            if (pointer + 2 > endOfSection)
            {
              throw new TvException("S-VCT: detected virtual channel map descriptor count {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", descriptorCount, pointer, endOfSection, i, d);
            }
            byte tag = section[pointer++];
            byte length = section[pointer++];
            this.LogDebug("S-VCT: virtual channel map descriptor, tag = 0x{0:x}, length = {1}", tag, length);
            if (pointer + length > endOfSection)
            {
              throw new TvException("S-VCT: invalid virtual channel map descriptor length {0}, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", length, pointer, endOfSection, i, d);
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
        throw new TvException("S-VCT: corruption detected at inverse channel map, pointer = {0}, end of section = {1}", pointer, endOfSection);
      }

      ushort firstMapIndex = (ushort)(((section[pointer] & 0x0f) << 8) | section[pointer + 1]);
      pointer += 2;
      byte recordCount = (byte)(section[pointer++] & 0x7f);
      if (pointer + (recordCount * 4) > endOfSection)
      {
        throw new TvException("S-VCT: invalid inverse channel map record count {0}, pointer = {1}, end of section = {2}", recordCount, pointer, endOfSection);
      }
      this.LogDebug("S-VCT: inverse channel map, first map index = {0}, record count = {1}", firstMapIndex, recordCount);
      for (byte i = 0; i < recordCount; i++)
      {
        ushort sourceId = (ushort)((section[pointer] << 8) | section[pointer + 1]);
        pointer += 2;
        ushort virtualChannelNumber = (ushort)(((section[pointer] & 0x0f) << 8) | section[pointer + 1]);
        pointer += 2;
        this.LogDebug("S-VCT: source ID = {0}, virtual channel number = {1}", sourceId, virtualChannelNumber);
      }
    }
  }
}