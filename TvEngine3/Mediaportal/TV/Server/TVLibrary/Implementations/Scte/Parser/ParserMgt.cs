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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Scte.Parser
{
  public delegate void MgtTableDetailDelegate(MgtTableType tableType, int pid, int versionNumber, uint byteCount);

  public class ParserMgt
  {
    private object _lock = new object();
    private int _currentVersion = -1;
    private HashSet<int> _unseenSections = new HashSet<int>();
    private TableCompleteDelegate _tableCompleteEventDelegate = null;
    private MgtTableDetailDelegate _tableDetailEventDelegate = null;

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

    public event MgtTableDetailDelegate OnTableDetail
    {
      add
      {
        lock (_lock)
        {
          _tableDetailEventDelegate += value;
        }
      }
      remove
      {
        lock (_lock)
        {
          _tableDetailEventDelegate -= value;
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
        if (_tableCompleteEventDelegate == null)
        {
          return;
        }
        if (section.Length < 19)
        {
          this.LogError("MGT: invalid section size {0}, expected at least 19 bytes", section.Length);
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
          this.LogError("MGT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
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
        this.LogDebug("MGT: section length = {0}, map ID = {1}, version number = {2}, section number = {3}, last section number {4}, protocol version = {5}, tables defined = {6}",
          sectionLength, mapId, versionNumber, sectionNumber, lastSectionNumber, protocolVersion, tablesDefined);

        int pointer = 13;
        int endOfSection = section.Length - 4;
        for (int i = 0; i < tablesDefined; i++)
        {
          if (pointer + 11 + 2 > endOfSection)  // + 2 for the fixed bytes after the loop
          {
            this.LogError("MGT: detected tables defined {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", tablesDefined, pointer, endOfSection, i);
            return;
          }
          MgtTableType tableType = (MgtTableType)((section[pointer] << 8) + section[pointer + 1]);
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
          this.LogDebug("MGT: table type = {0}, PID = {1}, version number = {2}", tableType, tableTypePid, tableTypeVersionNumber);
          if (_tableDetailEventDelegate != null)
          {
            _tableDetailEventDelegate(tableType, tableTypePid, tableTypeVersionNumber, numberBytes);
          }

          int tableTypeDescriptorsLength = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
          pointer += 2;
          int endOfTableTypeDescriptors = pointer + tableTypeDescriptorsLength;
          if (endOfTableTypeDescriptors > endOfSection)
          {
            this.LogError("MGT: invalid table type descriptors length {0}, pointer = {1}, end of section = {2}, loop = {3}", tableTypeDescriptorsLength, pointer, endOfSection, i);
            return;
          }
          while (pointer + 1 < endOfTableTypeDescriptors)
          {
            byte tag = section[pointer++];
            byte length = section[pointer++];
            this.LogDebug("MGT: table type descriptor, tag = 0x{0:x}, length = {1}", tag, length);
            if (pointer + length > endOfTableTypeDescriptors)
            {
              this.LogError("MGT: invalid table type descriptor length {0}, pointer = {1}, end of table type descriptors = {2}, loop = {3}", length, pointer, endOfTableTypeDescriptors, i);
              return;
            }
            pointer += length;
          }
          if (pointer != endOfTableTypeDescriptors)
          {
            this.LogError("MGT: corruption detected at end of table type descriptors, pointer = {0}, end of section = {1}, end of table type descriptors = {2}, loop = {3}", pointer, endOfSection, endOfTableTypeDescriptors, i);
            return;
          }
        }

        int descriptorsLength = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
        pointer += 2;
        if (pointer + descriptorsLength != endOfSection)
        {
          this.LogError("MGT: invalid descriptors length {0}, pointer = {1}, end of section = {2}", descriptorsLength, pointer, endOfSection);
          return;
        }
        while (pointer + 1 < endOfSection)
        {
          byte tag = section[pointer++];
          byte length = section[pointer++];
          this.LogDebug("MGT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            this.LogError("MGT: invalid descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection);
            return;
          }
          pointer += length;
        }

        if (pointer != endOfSection)
        {
          this.LogError("MGT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
          return;
        }

        _unseenSections.Remove(sectionNumber);
        if (_unseenSections.Count == 0 && _tableCompleteEventDelegate != null)
        {
          _tableCompleteEventDelegate(MgtTableType.Mgt);
          _tableCompleteEventDelegate = null;
          _tableDetailEventDelegate = null;
        }
      }
    }
  }
}