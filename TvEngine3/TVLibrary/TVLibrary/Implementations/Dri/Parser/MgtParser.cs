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

namespace TvLibrary.Implementations.Dri.Parser
{
  public enum MgtTableType
  {
    Mgt = -1,

    // ATSC A-65
    TvctCurrentNext1 = 0x0000,
    TvctCurrentNext0 = 0x0001,
    CvctCurrentNext1 = 0x0002,
    CvctCurrentNext0 = 0x0003,
    ChannelEtt = 0x0004,
    Dccsct = 0x0005,

    // SCTE 65
    SvctVcm = 0x0010,
    SvctDcm = 0x0011,
    SvctIcm = 0x0012,
    NitCds = 0x0020,
    NitMms = 0x0021,
    NttSns = 0x0030

    // ATSC A-65
    // 0x0100..0x017f EIT-0..EIT-127
    // 0x0200..0x027f ETT-0..ETT-127
    // 0x0301..0x03ff RRT, rating_region 0x01..0xff
    // 0x1400..0x14ff DCCT, doc_id 0x00..0xff

    // SCTE 65
    // 0x1000..0x10ff AEIT, mgt_tag 0x00..0xff
    // 0x1100..0x11ff AETT, mgt_tag 0x00..0xff
  }

  public delegate void MgtTableDetailDelegate(int tableType, int pid, int versionNumber, uint byteCount);

  public class MgtParser
  {
    private int _currentVersion = -1;
    private HashSet<int> _unseenSections = new HashSet<int>();
    public event TableCompleteDelegate OnTableComplete;
    public event MgtTableDetailDelegate OnTableDetail;

    public void Reset()
    {
      _currentVersion = -1;
    }

    public void Decode(byte[] section)
    {
      if (OnTableComplete == null)
      {
        return;
      }
      if (section.Length < 19)
      {
        Log.Log.Error("MGT: invalid section size {0}, expected at least 19 bytes", section.Length);
        return;
      }

      byte tableId = section[2];
      if (tableId != 0xc7)
      {
        return;
      }
      bool sectionSyntaxIndicator = ((section[3] & 0x80) != 0);
      bool privateIndicator = ((section[3] & 0x40) != 0);
      int sectionLength = ((section[3] & 0x0f) << 8) + section[4];
      if (section.Length != 2 + sectionLength + 3)
      {
        Log.Log.Error("MGT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
        return;
      }
      int mapId = (section[5] << 8) + section[6];
      int versionNumber = ((section[7] >> 1) & 0x1f);
      bool currentNextIndicator = ((section[7] & 0x80) != 0);
      if (!currentNextIndicator)
      {
        // Not applicable yet. Technically this should never happen.
        return;
      }
      byte sectionNumber = section[8];
      byte lastSectionNumber = section[9];
      int sectionKey = sectionNumber;
      if (versionNumber > _currentVersion || (_currentVersion == 31 && versionNumber < _currentVersion))
      {
        _currentVersion = versionNumber;
        _unseenSections.Clear();
        for (byte s = 0; s <= lastSectionNumber; s++)
        {
          _unseenSections.Add(s);
        }
      }
      else if (!_unseenSections.Contains(sectionKey))
      {
        // Already seen this section.
        return;
      }

      byte protocolVersion = section[10];
      int tablesDefined = (section[11] << 8) + section[12];
      Log.Log.Debug("MGT: section length = {0}, map ID = 0x{1:x}, version number = {2}, section number = {3}, last section number {4}, protocol version = {5}, tables defined = {6}",
        sectionLength, mapId, versionNumber, sectionNumber, lastSectionNumber, protocolVersion, tablesDefined);

      int pointer = 13;
      int endOfSection = section.Length - 4;
      for (int i = 0; i < tablesDefined; i++)
      {
        if (pointer + 11 + 2 > endOfSection)  // + 2 for the fixed bytes after the loop
        {
          Log.Log.Error("MGT: detected tables defined {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", tablesDefined, pointer, endOfSection, i);
          return;
        }
        int tableType = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        int tableTypePid = ((section[pointer] & 0x1f) << 8) + section[pointer + 1];
        pointer += 2;
        int tableTypeVersionNumber = (section[pointer++] & 0x1f);
        uint numberBytes = 0;
        for (byte b = 0; b < 4; b++)
        {
          numberBytes = numberBytes << 8;
          numberBytes = section[pointer++];
        }
        Log.Log.Debug("MGT: table type = 0x{0:x}, PID = 0x{1:x}, version number = {2}", tableType, tableTypePid, tableTypeVersionNumber);
        if (OnTableDetail != null)
        {
          OnTableDetail(tableType, tableTypePid, tableTypeVersionNumber, numberBytes);
        }

        int tableTypeDescriptorsLength = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
        pointer += 2;
        int endOfTableTypeDescriptors = pointer + tableTypeDescriptorsLength;
        if (endOfTableTypeDescriptors > endOfSection)
        {
          Log.Log.Error("MGT: invalid table type descriptors length {0}, pointer = {1}, end of section = {2}, loop = {3}", tableTypeDescriptorsLength, pointer, endOfSection, i);
          return;
        }
        while (pointer + 1 < endOfTableTypeDescriptors)
        {
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("MGT: table type descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfTableTypeDescriptors)
          {
            Log.Log.Error("MGT: invalid table type descriptor length {0}, pointer = {1}, end of table type descriptors = {2}, loop = {3}", length, pointer, endOfTableTypeDescriptors, i);
            return;
          }
          pointer += length;
        }
        if (pointer != endOfTableTypeDescriptors)
        {
          Log.Log.Error("MGT: corruption detected at end of table type descriptors, pointer = {0}, end of section = {1}, end of table type descriptors = {2}, loop = {3}", pointer, endOfSection, endOfTableTypeDescriptors, i);
          return;
        }
      }

      int descriptorsLength = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
      pointer += 2;
      if (pointer + descriptorsLength != endOfSection)
      {
        Log.Log.Error("MGT: invalid descriptors length {0}, pointer = {1}, end of section = {2}", descriptorsLength, pointer, endOfSection);
        return;
      }
      while (pointer + 1 < endOfSection)
      {
        byte tag = section[pointer++];
        byte length = section[pointer++];
        Log.Log.Debug("MGT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
        if (pointer + length > endOfSection)
        {
          Log.Log.Error("MGT: invalid descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection);
          return;
        }
        pointer += length;
      }

      if (pointer != endOfSection)
      {
        Log.Log.Error("MGT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
        return;
      }

      _unseenSections.Remove(sectionNumber);
      if (_unseenSections.Count == 0 && OnTableComplete != null)
      {
        OnTableComplete(MgtTableType.Mgt);
        OnTableComplete = null;
        OnTableDetail = null;
      }
    }
  }
}
