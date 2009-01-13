#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Globalization;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.TV.Database
{
  /// <summary>
  /// Class which holds all information about a recorded TV program
  /// </summary>
  [Serializable()]
  public class TVRecorded
  {
    public enum KeepMethod
    {
      UntilWatched,
      UntilSpaceNeeded,
      TillDate,
      Always
    } ;

    private long _startTime;
    private long _endTime;
    private string _title;
    private string _channelName;
    private string _genre;
    private string _description;
    private string _fileName;
    private int _recordedId = -1;
    private int _playedCounter = 0;
    private int _recordedCardIndex = 1;

    private DateTime _keepUntilDate = DateTime.MaxValue;
    private KeepMethod _keepUntilMethod = KeepMethod.UntilSpaceNeeded;

    public DateTime KeepRecordingTill
    {
      get { return _keepUntilDate; }
      set { _keepUntilDate = value; }
    }

    public KeepMethod KeepRecordingMethod
    {
      get { return _keepUntilMethod; }
      set { _keepUntilMethod = value; }
    }

    /// <summary>
    /// Property to get/set the filename of this recorded tv program
    /// </summary>
    public string FileName
    {
      get { return _fileName; }
      set { _fileName = value; }
    }

    /// <summary>
    /// Property to get/set the description of the recorded tv program 
    /// </summary>
    public string Description
    {
      get { return _description; }
      set { _description = value; }
    }

    /// <summary>
    /// Property to get/set the genre of the recorded tv program 
    /// </summary>
    public string Genre
    {
      get { return _genre; }
      set { _genre = value; }
    }

    /// <summary>
    /// Property to get/set the tv channel name of the recorded tv program 
    /// </summary>
    public string Channel
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    /// <summary>
    /// Property to get/set the title of the recorded tv program 
    /// </summary>
    public string Title
    {
      get { return _title; }
      set { _title = value; }
    }

    /// <summary>
    /// Property to get/set the start time of the recorded tv program in xmltv format :yyyymmddhhmmss
    /// </summary>
    public long Start
    {
      get { return _startTime; }
      set { _startTime = value; }
    }

    /// <summary>
    /// Property to get/set the end time of the recorded tv program in xmltv format :yyyymmddhhmmss
    /// </summary>
    public long End
    {
      get { return _endTime; }
      set { _endTime = value; }
    }

    /// <summary>
    /// Property to get the start time of the recorded tv program  
    /// </summary>
    public DateTime StartTime
    {
      get { return Util.Utils.longtodate(_startTime); }
    }

    /// <summary>
    /// Property to get the end time of the recorded tv program  
    /// </summary>
    public DateTime EndTime
    {
      get { return Util.Utils.longtodate(_endTime); }
    }

    /// <summary>
    /// Property to get/set the database ID of the recorded tv program  
    /// </summary>
    public int ID
    {
      get { return _recordedId; }
      set { _recordedId = value; }
    }

    /// <summary>
    /// Property to get/set how many times the record tv program has been watched
    /// </summary>
    public int Played
    {
      get { return _playedCounter; }
      set { _playedCounter = value; }
    }

    public void SetProperties()
    {
      string strTime = String.Format("{0} {1} - {2}",
                                     Util.Utils.GetShortDayString(StartTime),
                                     StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                     EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", Title);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", Genre);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", strTime);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", Description);
      string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, Channel);
      if (File.Exists(strLogo))
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", strLogo);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "defaultVideoBig.png");
      }
    }

    public bool ShouldBeDeleted
    {
      get
      {
        if (KeepRecordingMethod != KeepMethod.TillDate)
        {
          return false;
        }
        if (KeepRecordingTill.Date > DateTime.Now.Date)
        {
          return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Indicates on which card number the recording is made
    /// </summary>
    public int RecordedCardIndex
    {
      get { return _recordedCardIndex; }
      set { _recordedCardIndex = value; }
    }
  }
}