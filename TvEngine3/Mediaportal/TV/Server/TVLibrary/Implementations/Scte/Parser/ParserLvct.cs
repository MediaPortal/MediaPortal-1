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

using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Scte.Parser
{
  public enum ModulationMode : byte
  {
    Analog = 0x01,
    ScteMode1 = 0x02, // 64 QAM
    ScteMode2 = 0x03, // 256 QAM
    Atsc8Vsb = 0x04,
    Atsc16Vsb = 0x05,
    PrivateDescriptor = 0x80
  }

  public enum EtmLocation : byte
  {
    None,
    PhysicalChannelThis,
    PhysicalChannelTsid
  }

  public delegate void LvctChannelDetailDelegate(MgtTableType tableType, string shortName, int majorChannelNumber, int minorChannelNumber,
      ModulationMode modulationMode, uint carrierFrequency, int channelTsid, int programNumber, EtmLocation etmLocation,
      bool accessControlled, bool hidden, int pathSelect, bool outOfBand, bool hideGuide, AtscServiceType serviceType, int sourceId);

  /// <summary>
  /// ATSC/SCTE long form virtual channel table parser. Refer to ATSC A/65 and SCTE 65.
  /// </summary>
  public class ParserLvct
  {
    private object _lock = new object();
    private int _currentVersion = -1;
    private HashSet<int> _unseenSections = new HashSet<int>();
    private TableCompleteDelegate _tableCompleteEventDelegate = null;
    private LvctChannelDetailDelegate _channelDetailEventDelegate = null;

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

    public event LvctChannelDetailDelegate OnChannelDetail
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

    public void Reset()
    {
      lock (_lock)
      {
        _currentVersion = -1;
      }
    }

    public void Decode(byte[] section)
    {
      lock (_lock)
      {
        if (_tableCompleteEventDelegate == null || _channelDetailEventDelegate == null)
        {
          return;
        }

        if (section.Length < 18)
        {
          this.LogError("L-VCT: invalid section size {0}, expected at least 18 bytes", section.Length);
          return;
        }

        byte tableId = section[2];
        // 0xc8 = ATSC terrestrial, A/65
        // 0xc9 = SCTE cable, SCTE 65
        // The cable and terrestrial L-VCT formats are almost identical. The few
        // differences are noted below.
        if (tableId != 0xc8 && tableId != 0xc9)
        {
          return;
        }
        MgtTableType tableType = MgtTableType.CvctCurrentNext1;
        if (tableId == 0xc8)
        {
          tableType = MgtTableType.TvctCurrentNext1;
        }
        bool sectionSyntaxIndicator = ((section[3] & 0x80) != 0);
        bool privateIndicator = ((section[3] & 0x40) != 0);
        int sectionLength = ((section[3] & 0x0f) << 8) + section[4];
        if (section.Length != 2 + sectionLength + 3)  // 2 for section length bytes, 3 for table ID and PID
        {
          this.LogError("L-VCT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
          return;
        }
        int transportStreamId = (section[5] << 8) + section[6];
        int versionNumber = ((section[7] >> 1) & 0x1f);
        bool currentNextIndicator = ((section[7] & 0x80) != 0);
        if (!currentNextIndicator)
        {
          // Not applicable yet.
          return;
        }
        byte sectionNumber = section[8];
        byte lastSectionNumber = section[9];
        int sectionKey = (tableId << 8) + sectionNumber;
        if (versionNumber > _currentVersion || (_currentVersion == 31 && versionNumber < _currentVersion))
        {
          _currentVersion = versionNumber;
          _unseenSections.Clear();
          for (int s = 0; s <= lastSectionNumber; s++)
          {
            _unseenSections.Add((tableId << 8) + s);
          }
        }
        else if (!_unseenSections.Contains(sectionKey))
        {
          // Already seen this section.
          return;
        }

        byte protocolVersion = section[10];
        int numChannelsInSection = section[11];
        this.LogDebug("L-VCT: section length = {0}, transport stream ID = {1}, version number = {2}, section number = {3}, last section number {4}, protocol version = {5}, number of channels in section = {6}",
          sectionLength, transportStreamId, versionNumber, sectionNumber, lastSectionNumber, protocolVersion, numChannelsInSection);

        int pointer = 12;
        int endOfSection = section.Length - 4;
        for (int i = 0; i < numChannelsInSection; i++)
        {
          if (pointer + 32 + 2 > endOfSection)  // + 2 for the fixed bytes after the loop
          {
            this.LogError("L-VCT: detected number of channels in section {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", numChannelsInSection, pointer, endOfSection, i);
            return;
          }

          string shortName = System.Text.Encoding.Unicode.GetString(section, pointer, 14);
          pointer += 14;

          // SCTE cable supports both one-part and two-part channel numbers where
          // the major and minor channel number range is 0..999.
          // ATSC supports only two-part channel numbers where the major range is
          // 1..99 and the minor range is 0..999.
          // When the minor channel number is 0 it indicates an analog channel.
          int majorChannelNumber = ((section[pointer] & 0x0f) << 6) + (section[pointer + 1] >> 2);
          pointer++;
          int minorChannelNumber = ((section[pointer] & 0x03) << 8) + section[pointer + 1];
          pointer += 2;
          if ((majorChannelNumber & 0x03f0) == 0x03f0)
          {
            majorChannelNumber = ((majorChannelNumber & 0x0f) << 10) + minorChannelNumber;
            minorChannelNumber = 0;
          }

          ModulationMode modulationMode = (ModulationMode)section[pointer++];
          uint carrierFrequency = 0;   // Hz
          for (byte b = 0; b < 4; b++)
          {
            carrierFrequency = carrierFrequency << 8;
            carrierFrequency = section[pointer++];
          }
          int channelTsid = (section[pointer] << 8) + section[pointer + 1];
          pointer += 2;
          int programNumber = (section[pointer] << 8) + section[pointer + 1];
          pointer += 2;
          EtmLocation etmLocation = (EtmLocation)(section[pointer] >> 6);   // ATSC only, SCTE reserved
          bool accessControlled = ((section[pointer] & 0x20) != 0);
          bool hidden = ((section[pointer] & 0x10) != 0);
          int pathSelect = ((section[pointer] & 0x08) >> 3);                // SCTE only, ATSC reserved
          bool outOfBand = ((section[pointer] & 0x04) != 0);                // SCTE only, ATSC reserved
          bool hideGuide = ((section[pointer++] & 0x02) != 0);
          AtscServiceType serviceType = (AtscServiceType)(section[pointer++] & 0x3f);
          int sourceId = (section[pointer] << 8) + section[pointer + 1];
          pointer += 2;
          this.LogDebug("L-VCT: channel, short name = {0}, major channel number = {1}, minor channel number = {2}, modulation mode = {3}, carrier frequency = {4} Hz, TSID = {5}, program number = {6}, ETM location = {7}, access controlled = {8}, hidden = {9}, path select = {10}, out of band = {11}, hide guide = {12}, service type = {13}, source ID = {14}",
            shortName, majorChannelNumber, minorChannelNumber, modulationMode, carrierFrequency, channelTsid, programNumber,
            etmLocation, accessControlled, hidden, pathSelect, outOfBand, hideGuide, serviceType, sourceId);

          if (_channelDetailEventDelegate != null)
          {
            _channelDetailEventDelegate(tableType, shortName, majorChannelNumber, minorChannelNumber, modulationMode,
              carrierFrequency, channelTsid, programNumber, etmLocation, accessControlled, hidden, pathSelect, outOfBand,
              hideGuide, serviceType, sourceId);
          }

          int descriptorsLength = ((section[pointer] & 0x03) << 8) + section[pointer + 1];
          pointer += 2;
          int endOfDescriptors = pointer + descriptorsLength;
          if (endOfDescriptors > endOfSection)
          {
            this.LogError("L-VCT: invalid descriptors length {0}, pointer = {1}, end of section = {2}, loop = {3}", descriptorsLength, pointer, endOfSection, i);
            return;
          }
          while (pointer + 1 < endOfDescriptors)
          {
            byte tag = section[pointer++];
            byte length = section[pointer++];
            this.LogDebug("L-VCT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
            if (pointer + length > endOfDescriptors)
            {
              this.LogError("L-VCT: invalid descriptor length {0}, pointer = {1}, end of descriptors = {2}, loop = {3}", length, pointer, endOfDescriptors, i);
              return;
            }
            pointer += length;
          }
          if (pointer != endOfDescriptors)
          {
            this.LogError("L-VCT: corruption detected at end of descriptors, pointer = {0}, end of section = {1}, end of descriptors = {2}, loop = {3}", pointer, endOfSection, endOfDescriptors, i);
            return;
          }
        }

        int additionalDescriptorsLength = ((section[pointer] & 0x03) << 8) + section[pointer + 1];
        pointer += 2;
        if (pointer + additionalDescriptorsLength != endOfSection)
        {
          this.LogError("L-VCT: invalid additional descriptors length {0}, pointer = {1}, end of section = {2}", additionalDescriptorsLength, pointer, endOfSection);
          return;
        }
        while (pointer + 1 < endOfSection)
        {
          byte tag = section[pointer++];
          byte length = section[pointer++];
          this.LogDebug("L-VCT: additional descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            this.LogError("L-VCT: invalid additional descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection);
            return;
          }
          pointer += length;
        }

        if (pointer != endOfSection)
        {
          this.LogError("L-VCT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
          return;
        }

        _unseenSections.Remove(sectionKey);
        if (_unseenSections.Count == 0 && _tableCompleteEventDelegate != null)
        {
          _tableCompleteEventDelegate(tableType);
          _tableCompleteEventDelegate = null;
          _channelDetailEventDelegate = null;
        }
      }
    }
  }
}