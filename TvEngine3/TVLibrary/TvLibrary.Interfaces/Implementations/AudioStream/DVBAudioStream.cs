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
using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  [Serializable]
  public class DVBAudioStream : IAudioStream
  {
    #region variables
    string _language;
    AudioStreamType _streamType;
    int _pid;
    #endregion

    
    #region ctor
    public DVBAudioStream()
    {
      _language = "";
      _streamType = AudioStreamType.Mpeg2;
      _pid = 0;
    }
    #endregion

    #region properties
    /// <summary>
    /// gets/sets  Audio language
    /// </summary>
    public string Language 
    {
      get
      {
        return _language;
      }
      set
      {
        _language = value;
      }
    }

    /// <summary>
    /// gets/sets the audio stream type
    /// </summary>
    public AudioStreamType StreamType
    {
      get
      {
        return _streamType;
      }
      set
      {
        _streamType = value;
      }
    }

    /// <summary>
    /// gets/sets the audio pid for this stream
    /// </summary>
    public int Pid
    {
      get
      {
        return _pid;
      }
      set
      {
        _pid = value;
      }
    }
    #endregion

    public override bool Equals(object obj)
    {
      DVBAudioStream stream = obj as DVBAudioStream;
      if (stream == null) return false;
      if (_language == stream.Language && _streamType == stream.StreamType && _pid == stream.Pid) return true;
      return false;
    }
    public override string ToString()
    {
      return String.Format("pid:{0:X} language:{1} type:{2}",
        Pid, Language, StreamType);
    }
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _language.GetHashCode() ^_streamType.GetHashCode() ^ _pid.GetHashCode();
    }
  }
}
