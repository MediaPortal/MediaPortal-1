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

namespace Mediaportal.TV.Server.Plugins.TvMovieImport
{
  internal class ImportStats
  {
    public int ChannelCountTvmDb = 0;
    public int ProgramCountTvmDb = 0;
    public int ChannelCountTvmDbUnmapped = 0;
    public int ChannelCountTveDb = 0;
    public int ProgramCountTveDb = 0;

    public string GetTotalChannelDescription()
    {
      return string.Format("TV Movie database = {0}, unmapped = {1}, TV Server database = {2}", ChannelCountTvmDb, ChannelCountTvmDbUnmapped, ChannelCountTveDb);
    }
    public string GetTotalProgramDescription()
    {
      return string.Format("TV Movie database = {0}, TV Server database = {1}", ProgramCountTvmDb, ProgramCountTveDb);
    }
  }
}