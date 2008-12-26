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

using System;

namespace TvDatabase
{
  public class NowAndNext
  {
    int _idChannel;
    DateTime _nowStart;
    DateTime _nowEnd;
    string _titleNow;
    string _titleNext;
    int _idProgramNow;
    int _idProgramNext;

    public NowAndNext(int idChannel, DateTime nowStart, DateTime nowEnd, string titleNow, string titleNext, int idProgramNow, int idProgramNext)
    {
      _idChannel = idChannel;
      _nowStart = nowStart;
      _nowEnd = nowEnd;
      _titleNow = titleNow;
      _titleNext = titleNext;
      _idProgramNow = idProgramNow;
      _idProgramNext = idProgramNext;
    }

    public int IdChannel
    {
      get
      {
        return _idChannel;
      }
      set
      {
        _idChannel = value;
      }
    }

    public DateTime NowStartTime
    {
      get
      {
        return _nowStart;
      }
      set
      {
        _nowStart = value;
      }
    }

    public DateTime NowEndTime
    {
      get
      {
        return _nowEnd;
      }
      set
      {
        _nowEnd = value;
      }
    }

    public string TitleNow
    {
      get
      {
        return _titleNow;
      }
      set
      {
        _titleNow = value;
      }
    }

    public string TitleNext
    {
      get
      {
        return _titleNext;
      }
      set
      {
        _titleNext = value;
      }
    }

    public int IdProgramNow
    {
      get
      {
        return _idProgramNow;
      }
      set
      {
        _idProgramNow = value;
      }
    }

    public int IdProgramNext
    {
      get
      {
        return _idProgramNext;
      }
      set
      {
        _idProgramNext = value;
      }
    }
  }
}
