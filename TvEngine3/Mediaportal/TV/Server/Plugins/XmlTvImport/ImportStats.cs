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

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  internal class ImportStats
  {
    // Totals over all files:
    public int TotalChannelCountFiles = 0;
    public int TotalProgramCountFiles = 0;
    public int TotalChannelCountFilesUnmapped = 0;
    public int TotalProgramCountFilesUnmapped = 0;
    public int TotalChannelCountDb = 0;
    public int TotalProgramCountDb = 0;

    // For the current file:
    public int FileChannelCount = 0;
    public int FileProgramCount = 0;
    public int FileChannelCountUnmapped = 0;
    public int FileProgramCountUnmapped = 0;
    public int FileChannelCountDb = 0;
    public int FileProgramCountDb = 0;

    public void ResetFileStats()
    {
      FileChannelCount = 0;
      FileProgramCount = 0;
      FileChannelCountUnmapped = 0;
      FileProgramCountUnmapped = 0;
      FileChannelCountDb = 0;
      FileProgramCountDb = 0;
    }

    public string GetTotalChannelDescription()
    {
      return string.Format("file = {0}, unmapped = {1}, database = {2}", TotalChannelCountFiles, TotalChannelCountFilesUnmapped, TotalChannelCountDb);
    }
    public string GetTotalProgramDescription()
    {
      return string.Format("file = {0}, unmapped = {1}, database = {2}", TotalProgramCountFiles, TotalProgramCountFilesUnmapped, TotalProgramCountDb);
    }
  }
}