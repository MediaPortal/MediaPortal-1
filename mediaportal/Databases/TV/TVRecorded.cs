/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System;

using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
namespace MediaPortal.TV.Database
{
	/// <summary>
	/// Class which holds all information about a recorded TV program
	/// </summary>
	public class TVRecorded
	{
    long				m_iStartTime;
    long				m_iEndTime;
    string			m_strTitle;
    string      m_strChannel;
    string      m_strGenre;
    string      m_strDescription;
    string      m_strFilename;
    int         m_iID=-1;
    int         m_iPlayed=0;
    /// <summary>
    /// Property to get/set the filename of this recorded tv program
    /// </summary>
    public string FileName
    {
      get { return m_strFilename;}
      set { m_strFilename=value;}
    }

    /// <summary>
    /// Property to get/set the description of the recorded tv program 
    /// </summary>
    public string Description
    {
      get { return m_strDescription;}
      set { m_strDescription=value;}
    }

    /// <summary>
    /// Property to get/set the genre of the recorded tv program 
    /// </summary>
    public string Genre
    {
      get { return m_strGenre;}
      set { m_strGenre=value;}
    }

    /// <summary>
    /// Property to get/set the tv channel name of the recorded tv program 
    /// </summary>
    public string Channel
    {
      get { return m_strChannel;}
      set { m_strChannel=value;}
    }

    /// <summary>
    /// Property to get/set the title of the recorded tv program 
    /// </summary>
    public string Title
    {
      get { return m_strTitle;}
      set { m_strTitle=value;}
    }

    /// <summary>
    /// Property to get/set the start time of the recorded tv program in xmltv format :yyyymmddhhmmss
    /// </summary>
    public long Start
    {
      get { return m_iStartTime;}
      set { m_iStartTime=value;}
    }

    /// <summary>
    /// Property to get/set the end time of the recorded tv program in xmltv format :yyyymmddhhmmss
    /// </summary>
    public long End
    {
      get { return m_iEndTime;}
      set { m_iEndTime=value;}
    }

    /// <summary>
    /// Property to get the start time of the recorded tv program  
    /// </summary>
    public DateTime StartTime
    {
      get { return Utils.longtodate(m_iStartTime);}
    }

    /// <summary>
    /// Property to get the end time of the recorded tv program  
    /// </summary>
    public DateTime EndTime
    {
      get { return Utils.longtodate(m_iEndTime);}
    }

    /// <summary>
    /// Property to get/set the database ID of the recorded tv program  
    /// </summary>
    public int ID
    {
      get { return m_iID;}
      set { m_iID=value;}
    }

    /// <summary>
    /// Property to get/set how many times the record tv program has been watched
    /// </summary>
    public int Played
    {
      get { return m_iPlayed;}
      set { m_iPlayed=value;}
    }

		public void SetProperties()
		{
			string strTime=String.Format("{0} {1} - {2}", 
				Utils.GetShortDayString(StartTime) , 
				StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
				EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

			GUIPropertyManager.SetProperty("#TV.RecordedTV.Title",Title);
			GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre",Genre);
			GUIPropertyManager.SetProperty("#TV.RecordedTV.Time",strTime);
			GUIPropertyManager.SetProperty("#TV.RecordedTV.Description",Description);
			string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,Channel);
			if (System.IO.File.Exists(strLogo))
			{
				GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb",strLogo);
			}
			else
			{
				GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb","defaultVideoBig.png");
			}
		}
	}
}
