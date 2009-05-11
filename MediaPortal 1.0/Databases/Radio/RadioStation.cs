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

namespace MediaPortal.Radio.Database

{
	/// <summary>
	/// 
	/// </summary>
	public class RadioStation
	{
    int    m_ID=0;
    string m_strName="";
    int    m_iChannel=0;
    long   m_lFrequency=0;
    string m_strURL="";
    string m_strGenre=""; 
    int    m_iBitRate=0;
		bool   m_bScrambled=false;
    int _sort=40000;
    DateTime _lastTimeEpgGrabbed=DateTime.MinValue;
    int _epgHours = 2;

		public RadioStation()
		{
		}


    public DateTime LastDateTimeEpgGrabbed
    {
      get { return _lastTimeEpgGrabbed;}
      set { _lastTimeEpgGrabbed = value; }
    }
    public int EpgHours
    {
      get { return _epgHours; }
      set { _epgHours = value; }
    }
    public int ID
    {
      get { return m_ID;}
      set { m_ID=value;}
    }
    public int Sort
    {
      get { return _sort; }
      set { _sort = value; }
    }
    public string Name
    {
      get { return m_strName;}
      set { m_strName=value;}
    }
    public int Channel
    {
      get { return m_iChannel;}
      set { m_iChannel=value;}
    }
    public long Frequency
    {
      get { return m_lFrequency;}
      set { m_lFrequency=value;}
    }
    public string URL
    {
      get { return m_strURL;}
      set { m_strURL=value;}
    }
    public string Genre
    {
      get { return m_strGenre;}
      set { m_strGenre=value;}
    }
    public int BitRate
    {
      get { return m_iBitRate;}
      set { m_iBitRate=value;}
    }
		public bool Scrambled
		{
			get { return m_bScrambled;}
			set { m_bScrambled=value;}
		}

	}
}
