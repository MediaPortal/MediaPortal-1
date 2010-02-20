#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Text;
using TvLibrary.Interfaces;

namespace SetupTv.Sections
{
  /// <summary>
  /// Type of scan action
  /// </summary>
  public enum ScanTypes
  {
    Predefined = 0,
    SingleTransponder = 1,
    NIT = 2
  }

  /// <summary>
  /// Counter class
  /// </summary>
  internal class suminfo
  {
    private List<IChannel> _allNewChannels = new List<IChannel>();

    public List<IChannel> newChannels
    {
      get { return _allNewChannels; }
    }

    public int newChannel = 0;
    public int updChannel = 0;

    public int newChannelSum
    {
      get { return _allNewChannels.Count; }
    }

    public int updChannelSum = 0;
  }

  /// <summary>
  /// State of scanning
  /// </summary>
  public enum ScanState
  {
    Initialized,
    Scanning,
    Cancel,
    Done,
    Updating
  }
}