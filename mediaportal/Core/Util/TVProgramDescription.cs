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

namespace MediaPortal.Util
{
  /// <summary>
  /// Class which holds all details about a TV program
  /// </summary>
  public class TVProgramDescription
  {
    #region Variables

    private string _channelName = string.Empty;
    private string _genre = string.Empty;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private DateTime _startTime;
    private DateTime _endTime;

    #endregion

    #region Properties

    /// <summary>
    /// Property to get/set the name of this tv program
    /// </summary>
    public string Channel
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    /// <summary>
    /// Property to get/set the genre of this tv program
    /// </summary>
    public string Genre
    {
      get { return _genre; }
      set { _genre = value; }
    }

    /// <summary>
    /// Property to get/set the title of this tv program
    /// </summary>
    public string Title
    {
      get { return _title; }
      set { _title = value; }
    }

    /// <summary>
    /// Property to get/set the description of this tv program
    /// </summary>
    public string Description
    {
      get { return _description; }
      set { _description = value; }
    }

    /// <summary>
    /// Property to get the starttime of this tv program
    /// </summary>
    public DateTime StartTime
    {
      get { return _startTime; }
      set { _startTime = value; }
    }

    /// <summary>
    /// Property to get the endtime of this tv program
    /// </summary>
    public DateTime EndTime
    {
      get { return _endTime; }
      set { _endTime = value; }
    }

    #endregion
  }
}