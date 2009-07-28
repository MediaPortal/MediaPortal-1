/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
    private int _idChannel;
    private DateTime _nowStart;
    private DateTime _nowEnd;
    private string _titleNow;
    private string _titleNext;
    private int _idProgramNow;
    private int _idProgramNext;
    private string _episodeName;
    private string _episodeNameNext;
    private string _seriesNum;
    private string _seriesNumNext;
    private string _episodeNum;
    private string _episodeNumNext;
    private string _episodePart;
    private string _episodePartNext;

    public NowAndNext(int idChannel, DateTime nowStart, DateTime nowEnd, string titleNow, string titleNext,
                      int idProgramNow, int idProgramNext,
                      string episodeName, string episodeNameNext, string seriesNum, string seriesNumNext,
                      string episodeNum, string EpisodeNumNext, string episodePart, string episodePartNext)
    {
      _idChannel = idChannel;
      _nowStart = nowStart;
      _nowEnd = nowEnd;
      _titleNow = titleNow;
      _titleNext = titleNext;
      _idProgramNow = idProgramNow;
      _idProgramNext = idProgramNext;
      _episodeName = episodeName;
      _episodeNameNext = episodeNameNext;
      _seriesNum = seriesNum;
      _seriesNumNext = seriesNumNext;
      _episodeNum = episodeNum;
      _episodeNumNext = EpisodeNumNext;
      _episodePart = episodePart;
      _episodePartNext = episodePartNext;
    }

    public int IdChannel
    {
      get { return _idChannel; }
      set { _idChannel = value; }
    }

    public DateTime NowStartTime
    {
      get { return _nowStart; }
      set { _nowStart = value; }
    }

    public DateTime NowEndTime
    {
      get { return _nowEnd; }
      set { _nowEnd = value; }
    }

    public string TitleNow
    {
      get { return _titleNow; }
      set { _titleNow = value; }
    }

    public string TitleNext
    {
      get { return _titleNext; }
      set { _titleNext = value; }
    }

    public int IdProgramNow
    {
      get { return _idProgramNow; }
      set { _idProgramNow = value; }
    }

    public int IdProgramNext
    {
      get { return _idProgramNext; }
      set { _idProgramNext = value; }
    }

    public string EpisodeName
    {
      get { return _episodeName;}
      set { _episodeName = value;}
    }

    public string EpisodeNameNext
    {
      get { return _episodeNameNext; }
      set { _episodeNameNext = value; }
    }

    public string SeriesNum
    {
      get { return _seriesNum; }
      set { _seriesNum = value; }
    }

    public string SeriesNumNext
    {
      get { return _seriesNumNext; }
      set { _seriesNumNext = value; }
    }

    public string EpisodeNum
    {
      get { return _episodeNum; }
      set { _episodeNum = value; }
    }

    public string EpisodeNumNext
    {
      get { return _episodeNumNext; }
      set { _episodeNumNext = value; }
    }

    public string EpisodePart
    {
      get { return _episodePart; }
      set { _episodePart = value; }
    }

    public string EpisodePartNext
    {
      get { return _episodePartNext; }
      set { _episodePartNext = value; }
    }


  }
}