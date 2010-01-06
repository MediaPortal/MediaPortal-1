/* 
 *	Copyright (C) 2006-2009 Team MediaPortal
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

namespace TvLibrary.Channels
{
  /// <summary>
  /// class holding all tuning details for DVBIP
  /// </summary>
  [Serializable]
  public class DVBIPChannel : DVBBaseChannel
  {
    #region variables

    private string _url;

    #endregion

    /// <summary>
    /// URL of channel
    /// </summary>
    public string Url
    {
      get { return _url; }
      set { _url = value; }
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("DVBIP:{0} Url:{1}", base.ToString(), Url);
    }

    /// <summary>
    /// Comparision of channels
    /// </summary>
    /// <param name="obj">other channel to compare</param>
    /// <returns>true if equal</returns>
    public override bool Equals(object obj)
    {
      if ((obj as DVBIPChannel) == null) return false;
      if (!base.Equals(obj)) return false;
      DVBIPChannel ch = obj as DVBIPChannel;
      if (ch.Url != Url) return false;

      return true;
    }

    /// <summary>
    /// returns hashcode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _url.GetHashCode();
    }
  }
}