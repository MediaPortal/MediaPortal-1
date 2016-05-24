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

using System;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract]
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
    private int? _seasonNumber;
    private int? _seasonNumberNext;
    private int? _episodeNumber;
    private int? _episodeNumberNext;
    private int? _episodePartNumber;
    private int? _episodePartNumberNext;

    public NowAndNext(int idChannel, DateTime nowStart, DateTime nowEnd, string titleNow, string titleNext,
                      int idProgramNow, int idProgramNext,
                      string episodeName, string episodeNameNext, int? seasonNumber, int? seasonNumberNext,
                      int? episodeNumber, int? episodeNumberNext, int? episodePartNumber, int? episodePartNumberNext)
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
      _seasonNumber = seasonNumber;
      _seasonNumberNext = seasonNumberNext;
      _episodeNumber = episodeNumber;
      _episodeNumberNext = episodeNumberNext;
      _episodePartNumber = episodePartNumber;
      _episodePartNumberNext = episodePartNumberNext;
    }

    [DataMember]
    public int IdChannel
    {
      get { return _idChannel; }
    }

    [DataMember]
    public DateTime NowStartTime
    {
      get { return _nowStart; }
    }

    [DataMember]
    public DateTime NowEndTime
    {
      get { return _nowEnd; }
    }

    [DataMember]
    public string TitleNow
    {
      get { return _titleNow; }
    }

    [DataMember]
    public string TitleNext
    {
      get { return _titleNext; }
      set { _titleNext = value; }
    }

    [DataMember]
    public int IdProgramNow
    {
      get { return _idProgramNow; }
    }

    [DataMember]
    public int IdProgramNext
    {
      get { return _idProgramNext; }
      set { _idProgramNext = value; }
    }

    [DataMember]
    public string EpisodeName
    {
      get { return _episodeName; }
    }

    [DataMember]
    public string EpisodeNameNext
    {
      get { return _episodeNameNext; }
      set { _episodeNameNext = value; }
    }

    [DataMember]
    public int? SeasonNumber
    {
      get { return _seasonNumber; }
    }

    [DataMember]
    public int? SeasonNumberNext
    {
      get { return _seasonNumberNext; }
      set { _seasonNumberNext = value; }
    }

    [DataMember]
    public int? EpisodeNumber
    {
      get { return _episodeNumber; }
    }

    [DataMember]
    public int? EpisodeNumberNext
    {
      get { return _episodeNumberNext; }
      set { _episodeNumberNext = value; }
    }

    [DataMember]
    public int? EpisodePartNubmer
    {
      get { return _episodePartNumber; }
    }

    [DataMember]
    public int? EpisodePartNumberNext
    {
      get { return _episodePartNumberNext; }
      set { _episodePartNumberNext = value; }
    }
  }
}