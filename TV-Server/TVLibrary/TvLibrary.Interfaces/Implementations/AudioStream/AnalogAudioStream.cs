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
using DirectShowLib;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// class describing an analog audio stream
  /// </summary>
  [Serializable]
  public class AnalogAudioStream : IAudioStream
  {
    #region variables

    private string _language;
    private AudioStreamType _streamType;
    private TVAudioMode _audioMode;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalogAudioStream"/> class.
    /// </summary>
    public AnalogAudioStream()
    {
      _language = "";
      _streamType = AudioStreamType.Mpeg2;
      _audioMode = TVAudioMode.Stereo;
    }

    #endregion

    #region properties

    /// <summary>
    /// gets/sets  Audio language
    /// </summary>
    public string Language
    {
      get { return _language; }
      set { _language = value; }
    }

    /// <summary>
    /// gets/sets the audio stream type
    /// </summary>
    public AudioStreamType StreamType
    {
      get { return _streamType; }
      set { _streamType = value; }
    }

    /// <summary>
    /// Gets or sets the audio mode.
    /// </summary>
    /// <value>The audio mode.</value>
    public TVAudioMode AudioMode
    {
      get { return _audioMode; }
      set { _audioMode = value; }
    }

    #endregion

    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
    /// </returns>
    public override bool Equals(object obj)
    {
      AnalogAudioStream stream = obj as AnalogAudioStream;
      if (stream == null)
      {
        return false;
      }
      if (_language == stream.Language && _streamType == stream.StreamType && AudioMode == stream.AudioMode)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _language.GetHashCode() ^ _streamType.GetHashCode() ^ _audioMode.GetHashCode();
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return String.Format("mode:{0} type:{1} language:{2}",
                           AudioMode, StreamType, Language);
    }
  }
}