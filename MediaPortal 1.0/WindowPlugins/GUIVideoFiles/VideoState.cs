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

using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
  public class VideoState
  {
    static int m_iTempPlaylistWindow = 0;
    static string m_strTempPlaylistDirectory = "";
    static int m_iStartWindow = (int)GUIWindow.Window.WINDOW_VIDEOS;
    static string view;
    public VideoState()
    {
      // 
      // TODO: Add constructor logic here
      //
    }
    public static string View
    {
      get { return view; }
      set { view = value; }
    }

    public static string TempPlaylistDirectory
    {
      get { return m_strTempPlaylistDirectory; }
      set { m_strTempPlaylistDirectory = value; }
    }

    public static int TempPlaylistWindow
    {
      get { return m_iTempPlaylistWindow; }
      set { m_iTempPlaylistWindow = value; }
    }

    public static int StartWindow
    {
      get { return m_iStartWindow; }
      set { m_iStartWindow = value; }
    }
  }
}