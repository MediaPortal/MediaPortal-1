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
  public enum MgtTableType
  {
    Mgt = -1,

    // ATSC A/65
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

    // ATSC A/65
    // 0x0100..0x017f EIT-0..EIT-127
    // 0x0200..0x027f ETT-0..ETT-127
    // 0x0301..0x03ff RRT, rating_region 0x01..0xff
    // 0x1400..0x14ff DCCT, doc_id 0x00..0xff

    // SCTE 65
    // 0x1000..0x10ff AEIT, mgt_tag 0x00..0xff
    // 0x1100..0x11ff AETT, mgt_tag 0x00..0xff
  }

  public delegate void TableCompleteDelegate(MgtTableType table);

  public class ParserBase
  {
    protected object _lock = new object();
    protected Dictionary<int, int> _currentVersions = new Dictionary<int, int>();
    protected Dictionary<int, HashSet<int>> _unseenSections = new Dictionary<int, HashSet<int>>();
    protected int _firstTable = 0;
    protected int _lastTable = 0;

    public ParserBase(int firstTable, int lastTable)
    {
      _firstTable = firstTable;
      _lastTable = lastTable;
      for (int t = _firstTable; t <= _lastTable; t++)
      {
        _unseenSections[t] = new HashSet<int>();
      }
    }

    public virtual void Reset()
    {
      lock (_lock)
      {
        for (int t = _firstTable; t <= _lastTable; t++)
        {
          _currentVersions[t] = -1;
          _unseenSections[t].Clear();
        }
      }
    }

    public virtual void DecodeRevisionDetectionDescriptor(byte[] section, int pointer, byte length, int tableType)
    {
      if (length != 3)
      {
        throw new TvException("Invalid revision detection descriptor length {0}, pointer = {1}.", length, pointer);
      }

      int currentVersion = _currentVersions[tableType];
      HashSet<int> unseenSections = _unseenSections[tableType];

      int tableVersionNumber = (section[pointer++] & 0x1f);
      byte sectionNumber = section[pointer++];
      byte lastSectionNumber = section[pointer++];
      this.LogDebug("SCTE: revision detection descriptor, version = {0}, section number = {1}, last section number = {2}", tableVersionNumber, sectionNumber, lastSectionNumber);

      if (tableVersionNumber > currentVersion || (currentVersion == 31 && tableVersionNumber < currentVersion))
      {
        this.LogDebug("SCTE: new table version");
        _currentVersions[tableType] = tableVersionNumber;
        unseenSections.Clear();
        for (int s = 0; s <= lastSectionNumber; s++)
        {
          unseenSections.Add(s);
        }
        unseenSections.Remove(sectionNumber);
      }
      else
      {
        unseenSections.Remove(sectionNumber);
        this.LogDebug("SCTE: existing table version, unseen section count = {0}", unseenSections.Count);
      }
    }
  }
}