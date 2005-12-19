/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using Yeti.WMFSdk;

namespace MediaPortal.TagReader.WmaTagReader
{
  /// <summary>
  /// Summary description for WmaTagReader.
  /// </summary>
  public class WmaTagReader : ITagReader
  {
    private MusicTag m_tag = new MusicTag();
    public WmaTagReader()
    {
      // nothing to do here
    }

    public override int Priority
    {
      get { return 2; }
    }
    /// <summary>
    /// Returns true if the file is supported.
    /// </summary>
    /// <param name="strFileName">file to check if it is supported</param>
    /// <returns></returns>
    public override bool SupportsFile(string strFileName)
    {
      if (System.IO.Path.GetExtension(strFileName).ToLower() == ".wma") 
        return true;
      return false;
    }
    public override MusicTag Tag
    {
      get { return m_tag; }
    }
    public override bool ReadTag(String filename)
    {
      try
      { 
        // Clear old tag info
        m_tag.Clear();

        // Read new tag
        WmaStream str = new WmaStream(filename);

        try
        {
          m_tag.Title = str[WM.g_wszWMTitle];
        }
        catch (Exception) {}
        try
        {
          m_tag.Artist = str[WM.g_wszWMAlbumArtist];
        }
        catch (Exception) {}

        try
        {
          if (m_tag.Artist.Length == 0) m_tag.Artist = str[WM.g_wszWMOriginalArtist];
        }
        catch (Exception) {}

        try
        {
          if (m_tag.Artist.Length == 0) m_tag.Artist = str[WM.g_wszWMAuthor];
        }
        catch (Exception) {}

        try
        {
          if (m_tag.Artist.Length == 0) m_tag.Artist = str[WM.g_wszWMComposer];
        }
        catch (Exception) {}

        try
        {
          m_tag.Album = str[WM.g_wszWMAlbumTitle];
        }
        catch (Exception) {}
        try
        {
          m_tag.Genre = str[WM.g_wszWMGenre];
        }
        catch (Exception) {}
        if (m_tag.Title == null)
          m_tag.Title = "";
        if (m_tag.Artist == null)
          m_tag.Artist = "";
        if (m_tag.Album == null)
          m_tag.Album = "";
        if (m_tag.Genre == null)
          m_tag.Genre = "";

        string track = "";
        string year = "";
        string duration = "";

        try
        {
          track = str[WM.g_wszWMTrack];
        }
        catch (Exception) {}

        try
        {
          year = str[WM.g_wszWMYear];
        }
        catch (Exception) {}

        try
        { 
          duration = str[WM.g_wszWMDuration];
        } 
        catch (Exception) {}

        // handle track number; even if it is 3/16
        try
        {
          m_tag.Track = Convert.ToInt32(track);
        }
        catch
        {
          int k = 0;
          int l = 0;
          char[] trackChar = track.ToCharArray();
          for (l = 0; l < trackChar.Length; l++)
          {
            if (Char.IsDigit(trackChar[l])) break;
          }

          for (k = l; k < trackChar.Length; k++)
          {
            if (!Char.IsDigit(trackChar[k])) break;
          }
          if (l < k)
          {
            if (k == track.Length)
              m_tag.Track = Convert.ToInt32(track.Substring(l));
            else
              m_tag.Track = Convert.ToInt32(track.Substring(l, k - l));
          }
        }
        // convert the year string to int
        try
        {
          m_tag.Year = Convert.ToInt32(year);
        }
        catch
        {
          int k = 0;
          int l = 0;
          char[] yearChar = year.ToCharArray();
          for (l = 0; l < yearChar.Length; l++)
          {
            if (Char.IsDigit(yearChar[l])) break;
          }

          for (k = l; k < yearChar.Length; k++)
          {
            if (!Char.IsDigit(yearChar[k])) break;
          }
          if (l < k)
          {
            if (k == year.Length)
              m_tag.Year = Convert.ToInt32(year.Substring(l));
            else
              m_tag.Year = Convert.ToInt32(year.Substring(l, k - l));
          }
        }
        // convert the duration string to int.  The duration is in 100-nanoseconds
        try
        {
          m_tag.Duration = (int)(Convert.ToInt64(duration) / 10000000);
        }
        catch
        {
        }
      }
      catch
      {
        return false;
      }
      return true;
    }
  }
}
