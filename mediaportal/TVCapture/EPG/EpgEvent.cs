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
using System.Collections.Generic;
using System.Text;

using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Epg
{
  #region EPGEvent class
  class EPGEvent
  {
    private string _genre;
    private DateTime _startTime;
    private DateTime _endTime;
    List<EPGLanguage> _listLanguages = new List<EPGLanguage>();
    public EPGEvent(string genre, DateTime startTime, DateTime endTime)
    {
      _genre = genre;
      _startTime = startTime;
      _endTime = endTime;
    }
    public string Genre
    {
      get { return _genre; }
    }
    public DateTime StartTime
    {
      get { return _startTime; }
    }
    public DateTime EndTime
    {
      get { return _endTime; }
    }
    public List<EPGLanguage> Languages
    {
      get { return _listLanguages; }
    }
  }

  #endregion

}
