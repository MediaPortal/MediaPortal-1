#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

namespace MediaPortal.Utils.Time
{
  public class TimeRange
  {
    private BasicTime _start;
    private BasicTime _end;
    private bool _overMidnight;

    public TimeRange(DateTime start, DateTime end)
    {
      _start = new BasicTime(start);
      _end = new BasicTime(end);

      _overMidnight = false;
      if (_end.Hour < _start.Hour)
        _overMidnight = true;
    }

    public TimeRange(string start, string end)
    {
      _start = new BasicTime(start);
      _end = new BasicTime(end);

      _overMidnight = false;
      if (_end.Hour < _start.Hour)
        _overMidnight = true;
    }

    public bool IsInRange(DateTime checkTime)
    {
      if (_overMidnight)
      {
        if (_start < checkTime && checkTime.Hour < 24 ||
        _end > checkTime && checkTime.Hour > 0)
          return true;
      }
      else
      {
        if (_start < checkTime  && _end  > checkTime)
          return true;
      }
      return false;
    }

    public bool IsInRange(long time)
    {
      BasicTime checkTime = new BasicTime(time);

      if (_overMidnight)
      {
        if (_start < checkTime && checkTime.Hour < 24 ||
        _end > checkTime && checkTime.Hour > 0)
          return true;
      }
      else
      {
        if (_start < checkTime && _end > checkTime)
          return true;
      }
      return false;
    }

  }
}
