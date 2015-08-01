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
using Mediaportal.TV.Server.TVLibrary.Implementations.Scte.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Scte.Parser
{
  internal delegate void TableCompleteDelegate(MgtTableType table);

  internal class ParserBase
  {
    protected const int VERSION_NOT_DEFINED = -1;

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
          _currentVersions[t] = VERSION_NOT_DEFINED;
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